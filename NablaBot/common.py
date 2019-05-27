import queue
from threading import Thread, Event
import asyncio

class Singleton(type):
    def __init__(cls, name, bases, dict):
        super(Singleton, cls).__init__(name, bases, dict)
        cls.instance = None 

    def __call__(cls,*args,**kw):
        if cls.instance is None:
            cls.instance = super(Singleton, cls).__call__(*args, **kw)
        return cls.instance

#@Singleton
class MessageList(metaclass=Singleton):
    def __init__(self):
        self.size = 256
        self.queue = self.size * [None]
        self.head = 0

    def __getitem__(self, key):
        return queue[(head + key - 1) % self.size]

    def __iter__(self):
        self.iter = (head - 1) % self.size
        self.firstIter = True
        return self

    def __next__(self):
        message = self.queue[self.iter]
        if message is None or (self.iter == (self.head - 1) % self.size and not self.firstIter):
            raise StopIteration
        else:
            self.iter = (self.iter - 1) % self.size
            return message

    def push(self, message):
        self.queue[self.head] = message
        self.head = (self.head + 1) % self.size


#@Singleton
class SendQueue(Thread, metaclass=Singleton):
    def __init__(self, bot):
        Thread.__init__(self)
        self.stopped = Event()
        self.queue = queue.Queue(256)
        self._loop = asyncio.get_event_loop()
        self.wait_time = 1.5

    def run(self):
        while not self.stopped.wait(wait_time):
            if not self.queue.empty():
                message, context = self.queue.get()
                if message is not None:
                    asyncio.run_coroutine_threadsafe(context.send(message), self._loop)
                else:
                    self.wait_time = 1.5
                

    def put(self, message, context):
        if not self.queue.empty():
            self.queue.put((message, context))
        else:
            asyncio.run_coroutine_threadsafe(context.send(message), self._loop)
            self.queue.put((None, None))
            self.wait_time = 0.5

    def stop(self):
        self.stopped.set()