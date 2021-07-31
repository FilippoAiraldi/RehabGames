#pragma once

#include "net_common.h"
#include "net_concurrent_queue.h"
#include "net_message.h"
#include "net_connection.h"

namespace net
{
    template <typename T>
    class server_interface
    {
    public:
        server_interface(uint16_t port) : port(port), acceptor(context, asio::ip::tcp::endpoint(asio::ip::tcp::v4(), port))
        {
        }

        virtual ~server_interface()
        {
            this->stop();
        }

        bool start()
        {
            try
            {
                this->waitForClientConnectionAsync();
                this->contextThread = std::thread([this]() { this->context.run(); });
            }
            catch (const std::exception &e)
            {
                std::cerr << "[SERVER] Exception: " << e.what() << '\n';
                return false;
            }
            std::cout << "[SERVER] Started at port " << this->port << ".\n";
            return true;
        }

        bool stop()
        {
            this->context.stop();
            if (this->contextThread.joinable())
                this->contextThread.join();

            std::cout << "[SERVER] Stopped.\n";
            return true;
        }

        void sendMessage(std::shared_ptr<connection<T>> client, const message<T> &msg)
        {
            if (client && client->isConnected())
            {
                client->send(msg);
            }
            else
            {
                this->onClientDisconnected(client);
                this->removeConnection(client);
            }
        }

        void update(size_t maxMessages = -1, bool wait = false)
        {
            if (wait)
                this->msgsIn.wait();

            size_t msgsCnt = 0;
            while (msgsCnt < maxMessages && !this->msgsIn.empty())
            {
                owned_message<T> msg = this->msgsIn.pop_front();
                this->onMessage(msg.remote, msg.msg);
                msgsCnt++;
            }
        }

    protected:
        virtual bool onClientConnecting(std::shared_ptr<connection<T>> client)
        {
            return false;
        }

        virtual void onClientDisconnected(std::shared_ptr<connection<T>> client)
        {
        }

        virtual void onMessage(std::shared_ptr<connection<T>> client, T msg)
        {
        }

    protected:
        concurrent_queue<owned_message<T>> msgsIn; // thread-safe incoming msgs queue
        asio::io_context context;                  // for running asio stuff
        std::thread contextThread;                 // required by the asio context

        std::shared_ptr<connection<T>> current_client;

        uint16_t port;
        asio::ip::tcp::acceptor acceptor;
        uint32_t idCounter = 10000;

    protected:
        void waitForClientConnectionAsync()
        {
            this->acceptor.async_accept(
                [this](std::error_code ec, asio::ip::tcp::socket socket) {
                    if (!ec)
                    {
                        const asio::ip::tcp::endpoint newEndpoint = socket.remote_endpoint();

                        std::shared_ptr<connection<T>> newConnection = std::make_shared<connection<T>>(
                            connection<T>::owner::server,
                            this->context,
                            std::move(socket),
                            this->msgsIn);

                        if (this->onClientConnecting(newConnection))
                        {
                            this->current_client = std::move(newConnection);
                            this->current_client->connectToThisClient(this->idCounter++);

                            std::cout << "[SERVER] New connection " << newEndpoint
                                      << " approved with id " << this->current_client->getId() << ".\n";
                        }
                        else
                        {
                            std::cout << "[SERVER] New connection " << newEndpoint << " denied.\n";
                        }
                    }
                    else
                    {
                        std::cerr << "[SERVER] New connection error: " << ec.message() << "\n";
                    }

                    this->waitForClientConnectionAsync();
                });
        }

        void removeConnection(std::shared_ptr<connection<T>> client)
        {
            client->disconnect();
            client.reset();
            current_client.reset();
            this->msgsIn.clear();
        }
    };
} // namespace net
