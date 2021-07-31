#pragma once

#include "net_common.h"
#include "net_concurrent_queue.h"
#include "net_message.h"

namespace net
{
    template <typename T>
    class connection : public std::enable_shared_from_this<connection<T>>
    {
        // inheritance from std::enable_shared_from_this class
        // provides a shared pointer to "this", instead of a raw pointer

    public:
        enum class owner
        {
            server,
            client
        };

        connection(owner owner, asio::io_context &newContext, asio::ip::tcp::socket newSocket, concurrent_queue<owned_message<T>> &queue)
            : context(newContext), socket(std::move(newSocket)), msgsIn(queue)
        {
            this->ownerType = owner;
        }

        virtual ~connection() {}

        void connectToThisClient(uint32_t id)
        {
            if (this->ownerType != owner::server)
                return;
            if (this->socket.is_open())
            {
                this->id = id;
                this->readHeaderAsync();
            }
        }

        void connectToServer(const asio::ip::tcp::resolver::results_type &endpoints)
        {
            if (this->ownerType != owner::client)
                return;

            asio::async_connect(
                this->socket,
                endpoints,
                [this](std::error_code ec, asio::ip::tcp::endpoint ep) {
                    if (!ec)
                        this->readHeaderAsync();
                });
        }

        void disconnect()
        {
            if (this->isConnected())
                asio::post(this->context, [this]() { this->socket.close(); });
        }

        bool isConnected() const { return this->socket.is_open(); }
        uint32_t getId() const { return this->id; }

        friend std::ostream &operator<<(std::ostream &os, const connection<T> &conn)
        {
            os << conn.socket.remote_endpoint() << " (" << conn.id << ")";
            return os;
        }

    protected:
        asio::io_context &context;                  // context of whole asio
        asio::ip::tcp::socket socket;               // unique socket to a remote
        concurrent_queue<T> msgsOut;                // queue of msgs to be sent to remote
        concurrent_queue<owned_message<T>> &msgsIn; // queue of msgs sent by remote

        union
        {
            T val;
            char bytes[sizeof(T)];
        } tmpMsg;

        owner ownerType = owner::server;
        uint32_t id = 0;

    private:
        void readHeaderAsync()
        {
            if (!this->isConnected())
                return;

            auto on_complete = [this](std::error_code ec, std::size_t length) {
                this->addToIncomingMessageQueue();
            };
            asio::async_read(this->socket, asio::buffer(this->tmpMsg.bytes), on_complete);
        }

        void addToIncomingMessageQueue()
        {
            std::reverse(this->tmpMsg.bytes, this->tmpMsg.bytes + sizeof(T));
            owned_message<T> owned_msg;
            owned_msg.msg = this->tmpMsg.val;

            if (this->ownerType == owner::server)
                owned_msg.remote = this->shared_from_this();

            this->msgsIn.push_back(owned_msg);
            this->readHeaderAsync();
        }
    };
} // namespace net
