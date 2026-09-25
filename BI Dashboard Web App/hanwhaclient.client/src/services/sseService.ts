import { fetchEventSource } from "@microsoft/fetch-event-source";

export const connectSSE = (
    endpoint: string,
    onMessage: (data: string) => void
) => {

    const token = localStorage.getItem("accessToken");

    console.log("TOKEN:", token);

    const controller = new AbortController();

    fetchEventSource(endpoint, {

        method: "GET",

        headers: {
            Authorization: `Bearer ${token}`,
            Accept: "text/event-stream"
        },

        signal: controller.signal,

        async onopen(response) {

            console.log("STATUS:", response.status);

            if (!response.ok) {
                throw new Error(
                    `Connection failed: ${response.status}`
                );
            }

            console.log("SSE Connected");
        },

        onmessage(event) {

            console.log("DATA:", event.data);

            onMessage(event.data);
        },

        onerror(error) {

            console.error("SSE Error:", error);
        }
    });

    return () => controller.abort();
};