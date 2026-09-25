import { Box, List, ListItem, Typography } from '@mui/material';
import React from 'react'
import { IReferenceData, IserverData, IserverHealth } from '../../interfaces/IManageiDRAC';

interface serverListProps {
    server: IserverData[]; 
    referenceData : IReferenceData | undefined;
    selectedServer: IserverData | null;
    onSelectServer: (server: IserverData) => void;
    serverHealth : IserverHealth[]
    
}

const ServerList : React.FC<serverListProps> = ({ server,referenceData, selectedServer, onSelectServer,serverHealth}) => {

    const getLabel = (list: any[] = [], value: string | null) => {
        return list.find((item) => item.value === value)?.label || "";
    };

  
  return (
    <>
            <Box className="floor-plans-zones-left idrac-left">
                <div className="search-plans-and-jones search-plans-and-jones-report">
                    <List sx={{
                        maxHeight: 700,
                        overflowY: "auto",
                        overflowX: "hidden",
                        pr: 1,
                    }}>
                        {server?.length === 0 ? (
                            <Box
                                sx={{
                                    textAlign: "center",
                                    color: "gray",
                                    padding: "10px",
                                    fontSize: "16px",
                                }}
                            >
                                {"No server found"}
                            </Box>
                        ) : (
                            server?.map((item: IserverData) => {

                                const currentServerHealth = serverHealth?.find(
                                    (health) => health.ServerId === item.id
                                );

                                const showWarning =
                                    currentServerHealth?.Health !== "OK";

                                return (
                                    <React.Fragment key={item.id}>
                                        {(
                                            <ListItem
                                                key={item.id}
                                                className={`server-list-item ${selectedServer?.id === item.id ? "active" : ""
                                                    }`}
                                                 onClick={() => onSelectServer(item)}
                                            >
                                                <Typography className="server-name">
                                                    {item.serverName} 
                                                    {showWarning && (
                                                        <img
                                                            src="/images/iDRAC/Alert.gif"
                                                            alt="Alert"
                                                            style={{
                                                                width: 18,
                                                                height: 18,
                                                                objectFit: "contain",
                                                            }}
                                                        />
                                                    )}
                                                </Typography>
                                                {item.childSiteId !== null ?
                                                    <Box className="child-site-wrapper">
                                                        <span className='child-site-label'>Child site: </span>
                                                        <span className='child-site-name'> {getLabel(referenceData?.childSite, item.childSiteId)}</span>
                                                    </Box>
                                                    : ""}
                                                <Typography className='server-ip-port '>
                                                   {item.ipAddress} : {item.port}
                                                </Typography>


                                            </ListItem>
                                        )}
                                    </React.Fragment>
                                )
                            }
                            ))}
                    </List>

                </div>

               
            </Box>
        </>
  )
}

export default ServerList