import discord
from discord.ext import commands

if __name__ == '__main__':
    bot = commands.Bot(command_prefix='prefix')
    bot.run(open("../Tokens.txt", 'r').readlines()[3])