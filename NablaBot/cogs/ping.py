import discord
from discord.ext import commands

class PingCog(commands.Cog):

    def __init__(self, bot):
        self.bot = bot

    @commands.command(name='ping')
    async def ping_async(self, context):
        await context.send('pong')

    @commands.command(name='terminate')
    async def terminate_async(self, context):
        await self.bot.logout()

    @commands.command(name='say')
    async def say_async(self, context, *s):
        await context.send(' '.join(list(s)))

def setup(bot): bot.add_cog(PingCog(bot))