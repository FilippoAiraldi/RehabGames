#include "../../net/net.h"

typedef float message_t;

namespace BreakOut
{
    class Server : public net::server_interface<message_t>
    {
    public:
        Server(uint16_t port) : net::server_interface<message_t>(port)
        {
            restartRequested = false;
            stopRequested = false;
        }

        message_t command;
        bool restartRequested;
        bool stopRequested;

    protected:
        virtual bool onClientConnecting(std::shared_ptr<net::connection<message_t>> client)
        {
            restartRequested = true;
            std::cout << "Client connecting.\n";
            return true;
        }

        virtual void onClientDisconnected(std::shared_ptr<net::connection<message_t>> client)
        {
            stopRequested = true;
            this->removeConnection(client);
            std::cout << "Removing client [" << client->getId() << "]\n";
        }

        virtual void onMessage(std::shared_ptr<net::connection<message_t>> client, message_t msg)
        {
            command = msg;
            // std::cout << "Command received: " << command << "\n";
            if (msg == -1.0)
                this->onClientDisconnected(client);
        }
    };
}
