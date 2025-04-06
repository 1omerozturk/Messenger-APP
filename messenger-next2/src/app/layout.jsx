import { Inter } from "next/font/google";
import "./globals.css";
import Navbar from "./components/Navbar";
import { AuthProvider } from "@/contexts/AuthContext";
import { ChatProvider } from "@/contexts/ChatContext";
import { ThemeProvider } from "@/contexts/ThemeContext";

const inter = Inter({ subsets: ["latin"] });

export const metadata = {
  title: "Messenger App",
  description: "Real-time messaging application",
};

export default function RootLayout({ children }) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <AuthProvider>
          <ChatProvider>
            <ThemeProvider>
                <Navbar />
                <div className="h-16"></div>
              <main className="">
                {children}
              </main>
            </ThemeProvider>
          </ChatProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
