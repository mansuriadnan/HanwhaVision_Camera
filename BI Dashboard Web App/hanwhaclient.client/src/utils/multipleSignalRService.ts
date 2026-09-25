import * as signalR from "@microsoft/signalr";

const connections: Record<string, signalR.HubConnection> = {};

interface IServerList {
    id: string;
    hostingAddress: string;
    serverName: string;
    username: string;
    password: string;
    accessToken?: string;
    refreshToken?: string;
}

const serverConfigMap: Record<
    string,
    {
        token: string;
        baseUrl: string;
        connectionId: string;
    }
> = {};


export const initializeServer = async (server: IServerList) => {
    try {
        const tokens = await getServerToken(server);

        if (!tokens.accessToken) return null;

        await startMultipleSignalRConnection(
            server.id,
            server.hostingAddress,
            tokens.accessToken
        );

    } catch (error) {
        console.error("Init failed", error);

    }
};

export const getServerToken = async (server: IServerList) => {
    try {
        const res = await fetch(`${server.hostingAddress}/api/Auth/Login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                username: server.username,
                password: server.password,
            }),
        });

        const data = await res.json();

        return {
            accessToken: data?.data?.accessToken || "",
            refreshToken: data?.data?.refreshToken || "",
        };
    } catch (err) {
        console.error("Token fetch failed", err);
        return {
            accessToken: "",
            refreshToken: "",
        };
    }
};

export const startMultipleSignalRConnection = async (
    serverId: string,
    hostingAddress: string,
    token: string
) => {
    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${hostingAddress}/api/NotificationHub`, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect()
            .build();

        await connection.start();

        const connectionId = connection.connectionId ?? "";

       connections[serverId] = connection;

        serverConfigMap[serverId] = {
            token,
            baseUrl: hostingAddress,
            connectionId,
        };

        console.log(`✅ Connected to ${hostingAddress}`);
    } catch (err) {
        console.error(`❌ Failed to connect ${hostingAddress}`, err);
    }
};

export const multipleGetLiveData = async (
    widgetName: string,
    floorIds: string[],
    zoneIds: string[]
) => {
    const promises = Object.entries(serverConfigMap).map(
        async ([serverId, config]) => {
            const { token, baseUrl, connectionId } = config;

            if (!connectionId) {
                console.warn(`No connectionId for server ${serverId}`);
                return null;
            }

            try {
                const response = await multipleAPIPostService({
                    url: `${baseUrl}/api/Users/UserGroup`,
                    token,
                    data: {
                        ConnectionId: connectionId,
                        FloorId: floorIds,
                        zoneId: zoneIds,
                        widgetName,
                    },
                });

                return {
                    serverId,
                    data: response,
                };
            } catch (error) {
                console.error(`Error for server ${serverId}`, error);
                return {
                    serverId,
                    error,
                };
            }
        }
    );

    return await Promise.all(promises);
};

export const stopAllConnections = async () => {
    for (const key in connections) {
        await connections[key].stop();
    }
};

export const multipleOnReceiveMessage = (
  eventName: string,
  callback: (data: any) => void
) => {
  Object.values(connections).forEach((connection) => {
    if (!connection) return;

    connection.off(eventName);
    connection.on(eventName, callback);
  });
};


export const multipleAPIPostService = async <T>({
    url,
    data,
    token, 
}: {
    url: string;
    data: any;
    token?: string;
}): Promise<T> => {
    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(data),
        });

        return await response.json();
    } catch (error) {
        console.error("POST API error:", error);
        throw error;
    }
};

export const multipleLeftGroup = async () => {
  await Promise.all(
    Object.entries(serverConfigMap).map(
      async ([serverId, config]) => {
        const { token, baseUrl, connectionId } = config;

        if (!connectionId) {
          console.warn(`No connectionId for server ${serverId}`);
          return;
        }

        try {
          await multipleAPIPostService({
            url: `${baseUrl}/api/Users/RemoveUserGroup`,
            token,
            data: {
              id: connectionId,
            },
          });

          console.log(`✅ Left group for ${serverId}`);
        } catch (error) {
          console.error(`❌ Left group failed for ${serverId}`, error);
        }
      }
    )
  );
};