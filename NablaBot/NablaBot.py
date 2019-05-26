import discord
from discord.ext import commands
import glob

if __name__ == '__main__':

    bot = commands.Bot(command_prefix='##')

    # Load extensions from files
    for filename in glob.iglob('NablaBot\\cogs\\**\\*', recursive=True):
        if filename.split('.')[-1] == 'py':
            print('Loading extension ' + filename.split('.')[0].split('\\')[-1] + '...')
            bot.load_extension('cogs.' + filename.split('.')[0].split('\\')[-1])

    @bot.event
    async def on_ready():
        """http://discordpy.readthedocs.io/en/rewrite/api.html#discord.on_ready"""

        print(f'\n\nLogged in as: {bot.user.name} - {bot.user.id}\nVersion: {discord.__version__}\n')

        # Changes our bots Playing Status. type=1(streaming) for a standard game you could remove type and url.
        await bot.change_presence(activity=discord.Game(name='Testing Python', type=1, url='https://github.com/Origamijr/DiscordBots'))
        print(f'Successfully logged in and booted...!')

    # Run bot
    bot.run(open("Tokens.txt", 'r').readlines()[3].strip('\n'))