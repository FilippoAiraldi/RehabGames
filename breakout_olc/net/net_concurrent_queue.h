#pragma once

#include "net_common.h"

namespace net
{
    template <typename T>
    class concurrent_queue
    {
    public:
        concurrent_queue() = default;
        concurrent_queue(const concurrent_queue<T> &) = delete; // prevent copying since it has mutexes

        virtual ~concurrent_queue() { clear(); }

    public:
        const T &front()
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            return this->dq.front();
        }

        const T &back()
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            return this->dq.back();
        }

        T pop_front()
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            T item = std::move(this->dq.front());
            this->dq.pop_front();
            return item;
        }

        T pop_back()
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            T item = std::move(this->dq.back());
            this->dq.pop_back();
            return item;
        }

        void push_back(const T &item)
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            this->dq.push_back(std::move(item));

            const std::unique_lock<std::mutex> ul(this->mtxBlock);
            this->cv.notify_one();
        }

        void push_front(const T &item)
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            this->dq.push_front(std::move(item));

            const std::unique_lock<std::mutex> ul(this->mtxBlock);
            this->cv.notify_one();
        }

        bool empty()
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            return this->dq.empty();
        }

        size_t size()
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            return this->dq.size();
        }

        void clear()
        {
            const std::lock_guard<std::mutex> lock(this->mtxQueue);
            this->dq.clear();
        }

        void wait()
        {
            while (this->empty())
            {
                std::unique_lock<std::mutex> ul(this->mtxBlock);
                this->cv.wait(ul);
            }
        }

    protected:
        std::mutex mtxQueue;
        std::deque<T> dq;

        std::condition_variable cv;
        std::mutex mtxBlock;
    };
} // namespace net
