#define OLC_PGE_APPLICATION
#include "../../olc/olcPixelGameEngine.h"
#include "Server.h"

namespace BreakOut
{
    class Game : public olc::PixelGameEngine
    {
    public:
        Game(Server *gameServer) : server(gameServer), playing(false)
        {
            sAppName = "BreakOut";
        }

    private:
        Server *server;
        bool playing;

        olc::vf2d batPos, batDim;

        olc::vf2d ballPos, ballDir;
        float ballSpeed, ballRadius, ballAcceleration;

        olc::vi2d blockSize;
        std::unique_ptr<int[]> blocks;

        void Init()
        {
            batPos = {20.0f, float(ScreenHeight()) - blockSize.y * 5.0f};
            batDim = {60.0f, 10.0f};

            ballSpeed = 7.0f;
            ballRadius = 5.0f;
            ballAcceleration = 0.1f;

            // Start Ball - always pointing downwards
            float margin = 0.75f;
            float a = float(rand()) / float(RAND_MAX) * (3.14159f - 2 * margin) + margin;
            ballDir = {cos(a), sin(a)};
            ballPos = {12.5f, 13.5f};
        }

        void CreateWorld()
        {
            blockSize = {int(ScreenWidth() / 24.0f), int(ScreenHeight() / 30.0f)};
            blocks = std::make_unique<int[]>(24 * 30);
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 24; x++)
                {
                    if (x == 0 || y == 0 || x == 23 || y == 29)
                        blocks[y * 24 + x] = 10;
                    else
                        blocks[y * 24 + x] = 0;

                    if (x > 2 && x <= 20 && y > 3 && y <= 5)
                        blocks[y * 24 + x] = 1;
                    if (x > 2 && x <= 20 && y > 5 && y <= 7)
                        blocks[y * 24 + x] = 2;
                    if (x > 2 && x <= 20 && y > 7 && y <= 9)
                        blocks[y * 24 + x] = 3;
                }
            }
        }

        bool TestResolveCollisionPoint(const olc::vf2d &point, olc::vf2d potentialBallPos, olc::vf2d tileBallRadialDims)
        {
            olc::vi2d testPoint = potentialBallPos + tileBallRadialDims * point;

            auto &tile = blocks[testPoint.y * 24 + testPoint.x];
            if (tile == 0)
            {
                // Do Nothing, no collision
                return false;
            }
            else
            {
                // Ball has collided with a tile
                bool tileHit = tile < 10;
                if (tileHit)
                    tile--;

                // Collision response
                if (point.x == 0.0f)
                    ballDir.y *= -1.0f;
                if (point.y == 0.0f)
                    ballDir.x *= -1.0f;

                // randomize
                if (tile != 10)
                {
                    ballDir.x += (float(rand()) / float(RAND_MAX) - 0.5f) * 0.3f * tile;
                    ballDir.y += (float(rand()) / float(RAND_MAX) - 0.5f) * 0.3f * tile;
                    ballDir = ballDir.norm();
                }

                return tileHit;
            }
        }

        void showWaitingScreen()
        {
            Clear(olc::BLACK);
            DrawString({10, 10}, "Waiting for connection...");
            Init();
        }

        void runGame(float elapsedTime)
        {
            // Update Bat position as commanded by the server
            float p = std::max(0.0f, std::min(1.0f, server->command));
            batPos.x = blockSize.x + p * (ScreenWidth() - 2 * blockSize.x - batDim.x);

            // Calculate where ball should be, if no collision
            olc::vf2d potentialBallPos = ballPos + ballDir * ballSpeed * elapsedTime;

            // Test for hits 4 points around ball
            olc::vf2d tileBallRadialDims = {ballRadius / blockSize.x, ballRadius / blockSize.y};
            bool tileHit = false;
            tileHit |= TestResolveCollisionPoint(olc::vf2d(0, -1), potentialBallPos, tileBallRadialDims);
            tileHit |= TestResolveCollisionPoint(olc::vf2d(0, +1), potentialBallPos, tileBallRadialDims);
            tileHit |= TestResolveCollisionPoint(olc::vf2d(-1, 0), potentialBallPos, tileBallRadialDims);
            tileHit |= TestResolveCollisionPoint(olc::vf2d(+1, 0), potentialBallPos, tileBallRadialDims);

            // Actually update ball position with modified direction
            ballPos += ballDir * ballSpeed * elapsedTime;
            ballSpeed += ballAcceleration * elapsedTime;

            // Check Bat vs Ball collision
            olc::vf2d trueBallPos = {ballPos.x * float(blockSize.x), ballPos.y * float(blockSize.y)};
            if ((trueBallPos.y + ballRadius >= batPos.y) && (trueBallPos.x >= batPos.x) && (trueBallPos.x <= batPos.x + batDim.x))
            {
                // invert y
                ballDir.y *= -1.0f;

                // modulate x based on impact distance from bat center - delta goes from -1 to 1
                float delta = (trueBallPos.x - (batPos.x + batDim.x / 2.0f)) / (batDim.x / 2.0f) * (ballDir.x > 0.0f ? 1.0f : -1.0f);
                ballDir.x += ballDir.x * delta / 1.33f;
                ballDir = ballDir.norm();
            }

            // avoid zero horizontal velocity
            while (std::abs(ballDir.x) <= 0.005f)
            {
                ballDir.x += float(rand()) / float(RAND_MAX) - 0.5f;
                ballDir = ballDir.norm();
            }

            // avoid zero vertical velocity
            while (std::abs(ballDir.y) <= 0.005f)
            {
                ballDir.y -= (0.05f + float(rand()) / float(RAND_MAX));
                ballDir = ballDir.norm();
            }

            // Check if game lost - Ball below Bat
            if (trueBallPos.y - ballRadius > batPos.y + batDim.y)
            {
                CreateWorld(); // restart game
                Init();
            }

            // Draw Screen
            Clear(olc::VERY_DARK_BLUE);
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 24; x++)
                {
                    switch (blocks[y * 24 + x])
                    {
                    case 0:
                    case 11: // Do nothing
                        break;
                    case 10: // Draw Boundary
                        FillRect(olc::vi2d(x, y) * blockSize, blockSize, olc::GREY);
                        break;
                    case 1: // Draw Red Block
                        FillRect(olc::vi2d(x, y) * blockSize, blockSize, olc::RED);
                        DrawRect(olc::vi2d(x, y) * blockSize, blockSize, olc::DARK_RED);
                        break;
                    case 2: // Draw Green Block
                        FillRect(olc::vi2d(x, y) * blockSize, blockSize, olc::YELLOW);
                        DrawRect(olc::vi2d(x, y) * blockSize, blockSize, olc::DARK_YELLOW);
                        break;
                    case 3: // Draw Yellow Block
                        FillRect(olc::vi2d(x, y) * blockSize, blockSize, olc::GREEN);
                        DrawRect(olc::vi2d(x, y) * blockSize, blockSize, olc::DARK_GREEN);
                        break;
                    }
                }
            }

            // Draw Bat at the server command value
            FillRect(batPos, batDim, olc::GREY);

            // Draw Ball
            FillCircle(ballPos * blockSize, ballRadius, olc::GREY);
        }

    public:
        bool OnUserCreate() override
        {
            CreateWorld();
            Init();
            return true;
        }

        bool OnUserUpdate(float elapsedTime) override
        {
            // Poll server
            if (server->restartRequested)
            {
                server->restartRequested = false;
                playing = true;
                CreateWorld();
                Init();
            }
            if (server->stopRequested)
            {
                server->stopRequested = false;
                playing = false;
            }

            // Show waiting screen if not playing
            if (!playing)
            {
                showWaitingScreen();
            }
            else
            {
                runGame(elapsedTime);
            }

            return true;
        }
    };
}
