import discord
from discord.ext import commands
import sys
sys.path.insert(0,'..')
from common import MessageList, SendQueue

class PingCog(commands.Cog):

    def __init__(self, bot):
        self.bot = bot
        self.sendQueue = SendQueue()

    @commands.command(name='ping')
    async def ping_async(self, context):
        await context.send('pong')

    @commands.command(name='terminate')
    async def terminate_async(self, context):
        self.sendQueue.stop()
        await self.bot.logout()

    @commands.command(name='say')
    async def say_async(self, context, *s):
        self.sendQueue.put(' '.join(list(s)), context)

    @commands.command(name='repeat')
    async def repeat_async(self, context, *s):
        for query in s:
            if len(query) > 2 and query[:2] == "->" and query[2:].isDigit():
                pass

    @commands.Cog.listener()
    async def on_raw_reaction_add(self, payload):
        print(str(payload.user_id) + ' ' + str(payload.emoji) + '  ' + ('Unicode from ping 😠' if payload.emoji.is_unicode_emoji() else 'Not Unicode from ping 😠'))
        pass

def setup(bot): bot.add_cog(PingCog(bot))