import { createContext, useContext, useState, ReactNode } from "react";

type SignalRConnectionIDContextType = {
  ConnectionID: string;
  setConnectionIDValue: (data: string) => void;
};

const SignalRConnectionIDContext = createContext<SignalRConnectionIDContextType | undefined>(undefined);

export const SignalRConnectionIDProvider = ({ children }: { children: ReactNode }) => {
  const [ConnectionID, setConnectionID] = useState<string>("");

  const setConnectionIDValue = (data: string) => {
    setConnectionID(data);
  };

  return (
    <SignalRConnectionIDContext.Provider
      value={{
        ConnectionID,
        setConnectionIDValue,     
      }}
    >
      {children}
    </SignalRConnectionIDContext.Provider>
  );
};

export const useSignalRConnectionIDContext = () => {
  const context = useContext(SignalRConnectionIDContext);
  if (!context) {
    throw new Error("useSignalRConnectionIDContext must be used within a SignalRConnectionIDProvider");
  }
  return context;
};
