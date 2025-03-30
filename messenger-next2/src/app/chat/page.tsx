"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import { useChat } from "@/contexts/ChatContext";
import { format } from "date-fns";

// Konfirmasyon Dialog bileşeni
const ConfirmDialog = ({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
}: {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string;
}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 max-w-md w-full">
        <h3 className="text-lg font-semibold mb-4">{title}</h3>
        <p className="mb-6 text-gray-600">{message}</p>
        <div className="flex justify-end space-x-3">
          <button
            onClick={onClose}
            className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-100"
          >
            İptal
          </button>
          <button
            onClick={onConfirm}
            className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
          >
            Sil
          </button>
        </div>
      </div>
    </div>
  );
};

export default function Chat() {
  const router = useRouter();
  const { user, logout } = useAuth();
  const {
    conversations,
    currentConversation,
    messages,
    setCurrentConversation,
    sendMessage,
    loadMoreMessages,
    markAsRead,
    markAllAsRead,
    unreadCount,
    isLoading,
    error,
    allUsers,
    loadAllUsers,
    startNewConversation,
    deleteConversation,
  } = useChat();

  // Aktif sekmeyi izlemek için yeni state
  const [activeTab, setActiveTab] = useState<"chats" | "users">("chats");

  // Dialog için state'ler
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [conversationToDelete, setConversationToDelete] = useState<
    string | null
  >(null);

  useEffect(() => {
    if (!user) {
      router.push("/login");
    }
  }, [user, router]);

  useEffect(() => {
    if (currentConversation) {
      markAllAsRead();
    }
  }, [currentConversation, markAllAsRead]);

  const handleSendMessage = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.currentTarget;
    const input = form.elements.namedItem("message") as HTMLInputElement;
    const content = input.value.trim();

    if (content && currentConversation) {
      await sendMessage(content, currentConversation.userId);
      input.value = "";
    }
  };

  const handleDeleteClick = (userId: string, e: React.MouseEvent) => {
    e.stopPropagation(); // Tıklamanın yukarıya yayılmasını engelle
    setConversationToDelete(userId);
    setIsDeleteDialogOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (conversationToDelete) {
      const success = await deleteConversation(conversationToDelete);
      if (success) {
        // Başarılı silme sonrası işlemler (zaten Context içinde yapılıyor)
      }
    }

    // Dialog'u kapat
    setIsDeleteDialogOpen(false);
    setConversationToDelete(null);
  };

  if (!user) {
    return null;
  }

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">{error}</p>
          <button
            onClick={() => window.location.reload()}
            className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Confirm Dialog */}
      <ConfirmDialog
        isOpen={isDeleteDialogOpen}
        onClose={() => setIsDeleteDialogOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Sohbeti Sil"
        message="Bu sohbeti ve tüm mesajları silmek istediğinizden emin misiniz? Bu işlem geri alınamaz."
      />

      {/* Sidebar */}
      <div className="w-80 bg-white border-r">
        <div className="p-4 border-b">
          <div className="flex items-center justify-between">
              <img
                src={user.profilePicture || "/default-avatar.png"}
                alt={user.username}
                className="w-10 h-10 rounded-full"
              />
            <div>
              <h2 className="text-lg font-semibold">
                {user.firstName} {user.lastName}
              </h2>
              <p className="text-sm text-gray-500">{user.username}</p>
            </div>
            <button
              onClick={logout}
              className="text-sm text-red-600 hover:text-red-800"
            >
              Logout
            </button>
          </div>
        </div>

        {/* Tabs for Chats and Users */}
        <div className="flex border-b">
          <button
            className={`flex-1 py-3 text-sm font-medium ${
              activeTab === "chats"
                ? "text-indigo-600 border-b-2 border-indigo-600"
                : "text-gray-500 hover:text-gray-700"
            }`}
            onClick={() => setActiveTab("chats")}
          >
            Chats
          </button>
          <button
            className={`flex-1 py-3 text-sm font-medium ${
              activeTab === "users"
                ? "text-indigo-600 border-b-2 border-indigo-600"
                : "text-gray-500 hover:text-gray-700"
            }`}
            onClick={() => {
              setActiveTab("users");
              loadAllUsers(); // Kullanıcı sekmesine tıklandığında kullanıcıları yeniden yükleyelim
            }}
          >
            Users
          </button>
        </div>

        <div className="overflow-y-auto h-[calc(100vh-9rem)]">
          {activeTab === "chats" ? (
            // Conversations List
            conversations.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                No conversations yet
              </div>
            ) : (
              conversations.map((conversation, index) => (
                <div
                  key={`conv-${conversation.userId}-${index}`}
                  className={`p-4 cursor-pointer hover:bg-gray-50 ${
                    currentConversation?.userId === conversation.userId
                      ? "bg-gray-50"
                      : ""
                  }`}
                  onClick={() => setCurrentConversation(conversation)}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center flex-1">
                      <div className="relative">
                        <img
                          src={
                            conversation.profilePicture || "/default-avatar.png"
                          }
                          alt={conversation.username}
                          className="w-10 h-10 rounded-full"
                        />
                        {conversation.isOnline && (
                          <div className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 rounded-full border-2 border-white" />
                        )}
                      </div>
                      <div className="ml-3 flex-1">
                        <h3 className="text-sm font-medium">
                          {conversation.username}
                        </h3>
                        <p className="text-sm text-gray-500 truncate">
                          {conversation.lastMessage}
                        </p>
                      </div>
                      <div className="text-xs text-gray-500">
                        {format(
                          new Date(conversation.lastMessageTime),
                          "HH:mm"
                        )}
                      </div>
                    </div>

                    {/* Delete button */}
                    <button
                      onClick={(e) => handleDeleteClick(conversation.userId, e)}
                      className="ml-2 p-1 text-gray-400 hover:text-red-500"
                      title="Sohbeti Sil"
                      aria-label="Sohbeti Sil"
                    >
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        className="h-4 w-4"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                        />
                      </svg>
                    </button>
                  </div>

                  {conversation.unreadCount > 0 && (
                    <div className="mt-1">
                      <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-indigo-100 text-indigo-800">
                        {conversation.unreadCount}
                      </span>
                    </div>
                  )}
                </div>
              ))
            )
          ) : // All Users List
          allUsers.length === 0 ? (
            <div className="p-4 text-center text-gray-500">No users found</div>
          ) : (
            allUsers.map((user) => (
              <div
                key={`user-${user.id}`}
                className="p-4 cursor-pointer hover:bg-gray-50"
                onClick={() => {
                  startNewConversation(user.id);
                  setActiveTab("chats");
                }}
              >
                <div className="flex items-center">
                  <div className="relative">
                    <img
                      src={user.profilePicture || "/default-avatar.png"}
                      alt={user.username}
                      className="w-10 h-10 rounded-full"
                    />
                    {user.isOnline && (
                      <div className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 rounded-full border-2 border-white" />
                    )}
                  </div>
                  <div className="ml-3">
                    <h3 className="text-sm font-medium">{user.username}</h3>
                    <p className="text-sm text-gray-500">
                      {user.firstName} {user.lastName}
                    </p>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Chat Area */}
      <div className="flex-1 flex flex-col">
        {currentConversation ? (
          <>
            {/* Chat Header */}
            <div className="p-4 border-b bg-white flex justify-between items-center">
              <div className="flex items-center">
                <div className="relative">
                  <img
                    src={
                      currentConversation.profilePicture ||
                      "/default-avatar.png"
                    }
                    alt={currentConversation.username}
                    className="w-10 h-10 rounded-full"
                  />
                  {currentConversation.isOnline && (
                    <div className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 rounded-full border-2 border-white" />
                  )}
                </div>
                <div className="ml-3">
                  <h3 className="text-lg font-medium">
                    {currentConversation.username}
                  </h3>
                  <p className="text-sm text-gray-500">
                    {currentConversation.isOnline
                      ? "Online"
                      : `Last seen ${format(
                          new Date(currentConversation.lastSeen || ""),
                          "MMM d, HH:mm"
                        )}`}
                  </p>
                </div>
              </div>

              {/* Delete conversation button in header */}
              <button
                onClick={() => {
                  setConversationToDelete(currentConversation.userId);
                  setIsDeleteDialogOpen(true);
                }}
                className="text-gray-500 hover:text-red-500"
                title="Sohbeti Sil"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  className="h-5 w-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                  />
                </svg>
              </button>
            </div>

            {/* Messages */}
            <div className="flex-1 overflow-y-auto p-4 space-y-4">
              {messages.length === 0 ? (
                <div className="text-center text-gray-500">
                  No messages yet. Start the conversation!
                </div>
              ) : (
                messages.map((message, index) => (
                  <div
                    key={`${message.id}-${index}`}
                    className={`flex ${
                      message.senderId === user.id
                        ? "justify-end"
                        : "justify-start"
                    }`}
                  >
                    <div
                      className={`max-w-[70%] rounded-lg p-3 ${
                        message.senderId === user.id
                          ? "bg-indigo-600 text-white"
                          : "bg-white"
                      }`}
                    >
                      <p className="text-sm">{message.content}</p>
                      <p className="text-xs mt-1 opacity-70">
                        {format(new Date(message.createdAt), "HH:mm")}
                      </p>
                    </div>
                  </div>
                ))
              )}
            </div>

            {/* Message Input */}
            <form
              onSubmit={handleSendMessage}
              className="p-4 border-t bg-white"
            >
              <div className="flex space-x-2">
                <input
                  type="text"
                  name="message"
                  placeholder="Type a message..."
                  className="flex-1 rounded-lg border border-gray-300 px-4 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
                <button
                  type="submit"
                  className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                >
                  Send
                </button>
              </div>
            </form>
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center text-gray-500">
            Select a conversation to start chatting
          </div>
        )}
      </div>
    </div>
  );
}
