import discord
from discord.ext import commands
import sys
sys.path.insert(0,'..')
from common import MessageList, SendQueue

class PingCog(commands.Cog):

    def __init__(self, bot):
        self.bot = bot
        self.sendQueue = SendQueue()
        self.messageList = MessageList()
        self.mirrorMessage = None

    @commands.command(name='ping')
    async def ping_async(self, context):
        await context.send('pong')

    @commands.command(name='terminate')
    async def terminate_async(self, context):
        if context.author.id == 236746009688932354:
            self.sendQueue.stop()
            await self.bot.logout()
        else:
            self.sendQueue.put('Only Kevin can use this command...', context)

    @commands.command(name='say')
    async def say_async(self, context, *s):
        self.sendQueue.put(' '.join(list(s)), context)

    @commands.command(name='repeat')
    async def repeat_async(self, context, *s):
        message = ''
        for query in s:
            addon = None
            if len(query) > 2 and query[:2] == "->" and query[2:].isdigit():
                addon = self.messageList[int(query[2:]) - 1]
            else:
                addon = self.messageList.lastMessageWithContent(query)
            if addon is not None:
                message += addon.content + ' '
        self.sendQueue.put(message[:-1], context)

    @commands.command(name='mirror')
    async def mirror_async(self, context):
        self.sendQueue.put("Please add a reaction...", context)

    @commands.Cog.listener()
    async def on_message(self, message):
        if message.author.id == self.bot.user.id and message.content == "Please add a reaction...":
            self.mirrorMessage = message

    @commands.Cog.listener()
    async def on_raw_reaction_add(self, payload):
        if payload.message_id == self.mirrorMessage.id:
            await self.mirrorMessage.edit(content=str(payload.emoji))

def setup(bot): bot.add_cog(PingCog(bot))