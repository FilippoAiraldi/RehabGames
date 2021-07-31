#include <iostream>
#include <string>

#include "games/breakout/BreakOut.h"

void runServer(BreakOut::Server *server, size_t maxMessages, bool wait)
{
    server->start();
    while (1)
        server->update(maxMessages, wait);
}

void runGame(BreakOut::Game *game, int32_t screen_w, int32_t screen_h, int32_t pixel_sz)
{
    if (game->Construct(screen_w, screen_h, pixel_sz, pixel_sz))
        game->Start();
    else
        std::cout << "Game launch failed.\n";
}

int main(int argc, char *argv[])
{
    if (argc != 5)
    {
        std::cout << "Invalid number of arguments. Arguments must be:\n"
                  << "- server port\n"
                  << "- screen width\n"
                  << "- screen height\n"
                  << "- pixel size\n";
        system("pause");
        return -1;
    }

    // start server on a thread
    uint16_t port = atoi(argv[1]);
    BreakOut::Server *server = new BreakOut::Server(port);
    std::thread server_thread(runServer, server, -1, true);

    // start game
    BreakOut::Game game(server);
    int32_t screen_w = atoi(argv[2]);
    int32_t screen_h = atoi(argv[3]);
    int32_t pixel_sz = atoi(argv[4]);
    runGame(&game, screen_w, screen_h, pixel_sz);

    if (server_thread.joinable())
        server_thread.join();
    delete server;
    return 0;
}
