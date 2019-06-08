from .text_classify_utils.explain import Explainer
import discord
from discord.ext import commands

import sys
from os import path
sys.path.append( path.dirname( path.dirname( path.abspath(__file__) ) ) )
from common import SendQueue, MessageList, discord_format
from time import sleep

class ClassifyCog(commands.Cog):

    def __init__(self, bot):
        self.bot = bot
        self.sendQueue = SendQueue()
        self.messageList = MessageList()

        print('Loading sentiment data...')
        self.sentimentExplainer = Explainer(data="sentiment")

        print('Loading anime data...')
        self.animeExplainer = Explainer(data="anime_popularity")

        print('Loading anime audience data...')
        self.animeAudienceExplainer = Explainer(data="anime_audience")

        print('Loading anime theme data...')
        self.animeThemeExplainer = Explainer(data="anime_theme")
        
        print('Loading anime setting data...')
        self.animeSettingExplainer = Explainer(data="anime_setting")

        self.sentimentExplanation = dict()
        self.sentimentMessage = None
        self.sentimentMessageOriginal = []
        self.sentimentResult = None
        self.sentimentGraph = None
        self.sentimentGraphSent = False
        self.sentimentOriginalSet = False
        
        self.animeExplanation = dict()
        self.animeMessage = None
        self.animeMessageOriginal = []
        self.animeResult = None
        self.animeGraph = None
        self.animeGraphSent = False
        self.animeOriginalSet = False
        
        self.animeGenreExplanation = dict()
        self.animeGenreMessage = None
        self.animeGenreMessageOriginal = []
        self.animeGenreResult = None
        self.animeGenreGraph = None
        self.animeGenreGraphSent = False
        self.animeGenreOriginalSet = False

    def get_max_key_prob(self, d):
        maxProb = 0
        maxKey = ''
        for key, prob in d.items():
            if prob > maxProb:
                maxProb = prob
                maxKey = key
        return maxKey, maxProb

    @commands.command(name='text-classify-help')
    async def help_async(self, context, *s):
        self.sendQueue.put('```\n=== USAGE ===\n' + 
                            'Type one of the three commands below to prompt an analysis. You can click on the emojis below the copied message to see an overview of the explanation, or the emojis below the results to see more detailed graphs.\n\n' + 
                            '##sentiment - Sentimet analysis. Guesses whether the given text is POSITIVE or NEGATIVE\n\n' + 
                            '##anime-score - Guesses how successful an anime will be. An anime can be popular or not popular (based on number of members watched) and liked or not liked (based on score)\n\n' +
                            '##anime-genre - Guesses the genre of the anime. The genres guessed are [Seinen, Shounen, Shoujo, Josei], [Slice of Life, Fantasy, Supernatural, Sci-Fi], and [Action, Romance, Drama, Comedy]\n\n\n' + 
                            '--Extra--\n##anime-pitch - combines the bottom two commands (without interactivity) to give a brief review of your anime pitch! Try it out!\n```', context)

    @commands.command(name='sentiment')
    async def sentiment_async(self, context, *s):
        self.sentimentOriginalSet = False
        self.sentimentPrediction = self.sentimentExplainer.predict(' '.join(s))
        self.sentimentExplanation = self.sentimentExplainer.explain(' '.join(s))
        self.sendQueue.put(' '.join(s) + ' <SENTIMENT_MESSAGE>', context)
        maxKlass, maxProb = self.get_max_key_prob(self.sentimentPrediction)
        self.sendQueue.put('<SENTIMENT_RESULT> This statement is ***' + maxKlass + '*** with probability ' + str(maxProb) + '.', context)
        pass

    @commands.command(name='anime-score')
    async def anime_score_async(self, context, *s):
        self.animeOriginalSet = False
        self.animePrediction = self.animeExplainer.predict(' '.join(s))
        self.animeExplanation = self.animeExplainer.explain(' '.join(s))
        self.sendQueue.put(' '.join(s) + ' <ANIME_CLASS_MESSAGE>', context)
        maxKlass, maxProb = self.get_max_key_prob(self.animePrediction)
        self.sendQueue.put('<ANIME_CLASS_RESULT> This anime will be ***' + maxKlass + '*** with probability ' + str(maxProb) + '.', context)
        pass

    @commands.command(name='anime-genre')
    async def anime_genre_async(self, context, *s):
        self.animeGenreOriginalSet = False
        self.animeGenrePrediction = (self.animeAudienceExplainer.predict(' '.join(s)), self.animeThemeExplainer.predict(' '.join(s)), self.animeSettingExplainer.predict(' '.join(s)))
        self.animeGenreExplanation = (self.animeAudienceExplainer.explain(' '.join(s)), self.animeThemeExplainer.explain(' '.join(s)), self.animeSettingExplainer.explain(' '.join(s)))
        self.sendQueue.put(' '.join(s) + ' <ANIME_GENRE_MESSAGE>', context)
        maxAudience, maxAProb = self.get_max_key_prob(self.animeGenrePrediction[0])
        maxTheme, maxTProb = self.get_max_key_prob(self.animeGenrePrediction[1])
        maxSetting, maxSProb = self.get_max_key_prob(self.animeGenrePrediction[2])
        self.sendQueue.put('<ANIME_GENRE_RESULT> This seems to be a ***' + maxAudience + ' ' + maxSetting + ' ' + maxTheme + '*** with probability ' + str(maxAProb * maxSProb * maxTProb) + '.', context)
        pass

    @commands.command(name='anime-pitch')
    async def anime_pitch_async(self, context, *s):
        popularityExplanation = self.animeExplainer.explain(' '.join(s))
        popularityPrediction = self.animeExplainer.predict(' '.join(s))
        audiencePrediction = self.animeAudienceExplainer.predict(' '.join(s))
        themePrediction = self.animeThemeExplainer.predict(' '.join(s))
        settingPrediction = self.animeSettingExplainer.predict(' '.join(s))
        
        maxPopularity, maxPProb = self.get_max_key_prob(popularityPrediction)
        maxAudience, maxAProb = self.get_max_key_prob(audiencePrediction)
        maxTheme, maxTProb = self.get_max_key_prob(themePrediction)
        maxSetting, maxSProb = self.get_max_key_prob(settingPrediction)

        if len(popularityExplanation['Popular and Liked']) < 6:
            if maxPopularity == 'Popular and Liked':
                self.sendQueue.put("Based on what you've said, it seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. Despite your short summary, It does seem like your idea will be popular and well liked!", context)
            elif maxPopularity == 'Popular but not Liked':
                self.sendQueue.put("Based on what you've said, it seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. Your idea might gather an audience, but not too many of them will probably like it. Please include more details if you want suggestions", context)
            elif maxPopularity == 'not Popular but Liked':
                self.sendQueue.put("Based on what you've said, it seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. People will probably like your idea, but there probably won't be too many. Please include more details if you want suggestions", context)
            elif maxPopularity == 'neither Popular nor Liked':
                self.sendQueue.put("Based on what you've said, it seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. You idea sucks. You should consider revising it. Please include more details if you want suggestions", context)
        else:
            sortedExplanations = [(score, key) for key, score in popularityExplanation['Popular and Liked'].items()]
            sortedExplanations.sort()
            lowWords = [key for score, key in sortedExplanations[0:3] if score < 0]
            hiWords = [key for score, key in sortedExplanations[-4:-1] if score > 0]
            lowStr = (('"' + lowWords[-3] + '", ') if len(lowWords) > 2 else '') + (('"' + lowWords[-2] + '" and ') if len(lowWords) > 1 else '') + (('"' + lowWords[-1] + '"') if len(lowWords) > 0 else '')
            hiStr = (('"' + hiWords[-3] + '", ') if len(hiWords) > 2 else '') + (('"' + hiWords[-2] + '" and ') if len(hiWords) > 1 else '') + (('"' + hiWords[-1] + '"') if len(hiWords) > 0 else '')

            if maxPopularity == 'Popular and Liked':
                self.sendQueue.put("Nice Job! It seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. It does seem like your idea will be popular and well liked! Some words that stood out are ***" + hiStr + '***.', context)
            elif maxPopularity == 'Popular but not Liked':
                self.sendQueue.put("Well, it seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. Your idea might gather an audience, but not too many of them will probably like it. I liked when you said ***" + hiStr + "***, but not when you said ***" + lowStr + "***.", context)
            elif maxPopularity == 'not Popular but Liked':
                self.sendQueue.put("Hmm... it seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. People will probably like your idea, but there probably won't be too many. I liked when you said ***" + hiStr + "***, but not when you said ***" + lowStr + "***.", context)
            elif maxPopularity == 'neither Popular nor Liked':
                self.sendQueue.put("It seems like you are pitching a ***" + maxAudience + ' ' + maxSetting + ' ' + maxTheme + "***. I don't think your idea well take off well at all though. I liked when you said ***" + hiStr + "***, but not when you said ***" + lowStr + "***.", context)

    @commands.Cog.listener()
    async def on_message(self, message):
        if message.author.id == self.bot.user.id:
            tag = message.content.split(' ')[0]
            tag2 = message.content.split(' ')[-1]
            if tag2 == '<SENTIMENT_MESSAGE>':
                #sleep(1)
                self.sentimentMessage = message
                if not self.sentimentOriginalSet:
                    self.sentimentMessageOriginal = message.content.split(' ')[:-1]
                    self.sentimentOriginalSet = True
                await self.sentimentMessage.edit(content=' '.join(message.content.split(' ')[:-1]))
                await self.sentimentMessage.add_reaction('👍')
                await self.sentimentMessage.add_reaction('👎')
            elif tag == '<SENTIMENT_RESULT>':
                self.sentimentResult = message
                self.sentimentGraphSent = False
                await self.sentimentResult.edit(content=' '.join(message.content.split(' ')[1:]))
                await self.sentimentResult.add_reaction('📊')
                await self.sentimentResult.add_reaction('💬')
            elif tag == '<SENTIMENT_EXPLANATION_GRAPH>':
                self.sentimentGraph = message
                await self.sentimentGraph.edit(content=' '.join(message.content.split(' ')[1:]))
                await self.sentimentGraph.add_reaction('👍')
                await self.sentimentGraph.add_reaction('👎')

            elif tag2 == '<ANIME_CLASS_MESSAGE>':
                #sleep(1)
                self.animeMessage = message
                if not self.animeOriginalSet:
                    self.animeMessageOriginal = message.content.split(' ')[:-1]
                    self.animeOriginalSet = True
                await self.animeMessage.edit(content=' '.join(message.content.split(' ')[:-1]))
                await self.animeMessage.add_reaction('😃')
                await self.animeMessage.add_reaction('😠')
                await self.animeMessage.add_reaction('😤')
                await self.animeMessage.add_reaction('🥔')
            elif tag == '<ANIME_CLASS_RESULT>':
                self.animeResult = message
                self.animeGraphSent = False
                await self.animeResult.edit(content=' '.join(message.content.split(' ')[1:]))
                await self.animeResult.add_reaction('📊')
                await self.animeResult.add_reaction('💬')
            elif tag == '<ANIME_CLASS_EXPLANATION_GRAPH>':
                self.animeGraph = message
                await self.animeGraph.edit(content=' '.join(message.content.split(' ')[1:]))
                await self.animeGraph.add_reaction('😃')
                await self.animeGraph.add_reaction('😠')
                await self.animeGraph.add_reaction('😤')
                await self.animeGraph.add_reaction('🥔')
                
            elif tag2 == '<ANIME_GENRE_MESSAGE>':
                #sleep(1)
                self.animeGenreMessage = message
                if not self.animeGenreOriginalSet:
                    self.animeGenreMessageOriginal = message.content.split(' ')[:-1]
                    self.animeGenreOriginalSet = True
                await self.animeGenreMessage.edit(content=' '.join(message.content.split(' ')[:-1]))
                await self.animeGenreMessage.add_reaction('👵')
                await self.animeGenreMessage.add_reaction('👨')
                await self.animeGenreMessage.add_reaction('👧')
                await self.animeGenreMessage.add_reaction('👦')
                await self.animeGenreMessage.add_reaction('🏡')
                await self.animeGenreMessage.add_reaction('🐲')
                await self.animeGenreMessage.add_reaction('👻')
                await self.animeGenreMessage.add_reaction('🤖')
                await self.animeGenreMessage.add_reaction('🔫')
                await self.animeGenreMessage.add_reaction('❤')
                await self.animeGenreMessage.add_reaction('🎭')
                await self.animeGenreMessage.add_reaction('😂')
            elif tag == '<ANIME_GENRE_RESULT>':
                self.animeGenreResult = message
                self.animeGenreGraphSent = False
                await self.animeGenreResult.edit(content=' '.join(message.content.split(' ')[1:]))
                await self.animeGenreResult.add_reaction('📊')
                await self.animeGenreResult.add_reaction('💬')
            elif tag == '<ANIME_GENRE_EXPLANATION_GRAPH>':
                self.animeGenreGraph = message
                await self.animeGenreGraph.edit(content=' '.join(message.content.split(' ')[1:]))
                await self.animeGenreGraph.add_reaction('👵')
                await self.animeGenreGraph.add_reaction('👨')
                await self.animeGenreGraph.add_reaction('👧')
                await self.animeGenreGraph.add_reaction('👦')
                await self.animeGenreGraph.add_reaction('🏡')
                await self.animeGenreGraph.add_reaction('🐲')
                await self.animeGenreGraph.add_reaction('👻')
                await self.animeGenreGraph.add_reaction('🤖')
                await self.animeGenreGraph.add_reaction('🔫')
                await self.animeGenreGraph.add_reaction('❤')
                await self.animeGenreGraph.add_reaction('🎭')
                await self.animeGenreGraph.add_reaction('😂')

    def emoji_to_klass(self, emoji):
        if   emoji == '😃': return 'Popular and Liked'
        elif emoji == '😠': return 'Popular but not Liked'
        elif emoji == '😤': return 'not Popular but Liked'
        elif emoji == '🥔': return 'neither Popular nor Liked'

        elif emoji == '👵': return 'Josei'
        elif emoji == '👨': return 'Seinen'
        elif emoji == '👧': return 'Shoujo'
        elif emoji == '👦': return 'Shounen'
        elif emoji == '🔫': return 'Action'
        elif emoji == '❤': return 'Romance'
        elif emoji == '🎭': return 'Drama'
        elif emoji == '😂': return 'Comedy'
        elif emoji == '🏡': return 'Slice of Life'
        elif emoji == '🐲': return 'Fantasy'
        elif emoji == '👻': return 'Supernatural'
        elif emoji == '🤖': return 'Sci-Fi'

        elif emoji == '👍': return 'POSITIVE'
        elif emoji == '👎': return 'NEGATIVE'
        else: return None

    def graph_explanation(self, explanation, initial_label):
        maxScore = 0
        maxLength = 0
        graphWidth = 30
        for key, score in explanation[initial_label].items():
            if abs(score) > maxScore: maxScore = abs(score)
            if len(key) > maxLength: maxLength = len(key)
        while True:
            graph = "```cs\n=== " + initial_label + " ===\n"
            sortedExplanations = [(score, key) for key, score in explanation[initial_label].items()]
            sortedExplanations.sort(reverse=True)
            for score, key in sortedExplanations:
                graph += (maxLength - len(key)) * ' ' + key + ' | ' + (' ' if score >= 0 else '') + f'{score:5.4f}' + ' | '
                graph += int(abs(score) / maxScore * (graphWidth - 1) + 1) * ('#' if score < 0 else '0') + '\n'
            graph += "```"
            if len(graph) < 1964: break
            graphWidth -= 1
            if graphWidth < 15: break
        return graph

    def graph_prediction(self, prediction):
        maxProb = 0
        maxLength = 0
        graphWidth = 30
        for key, prob in prediction.items():
            if prob > maxProb: maxProb = prob
            if len(key) > maxLength: maxLength = len(key)
        while True:
            graph = "```cs\n"
            for key, prob in prediction.items():
                graph += (maxLength - len(key)) * ' ' + key + ' | ' + f'{prob:5.4f}' + ' | '
                graph += int(prob / maxProb * (graphWidth - 1) + 1) * '#' + '\n'
            graph += "```"
            if len(graph) < 1964: break
            graphWidth -= 1
            if graphWidth < 15: break
        return graph

    def split_graph(self, graph):
        if len(graph) < 1964:
            return [graph]
        splitPoint = 0
        for i in range(0, len(graph)):
            if graph[i] == '\n':
                splitPoint = i
            if i >= 1964:
                return [graph[:splitPoint] + '```'] + self.split_graph('```cs' + graph[splitPoint:])

    @commands.Cog.listener()
    async def on_raw_reaction_add(self, payload):
        if payload.user_id != self.bot.user.id:
            # Sentiment Reactions ==============================================
            if self.sentimentMessage is not None and payload.message_id == self.sentimentMessage.id:
                maxScore = 0
                for key, score in self.sentimentExplanation[self.emoji_to_klass(str(payload.emoji))].items():
                    if abs(score) > maxScore: maxScore = abs(score)
                def sentiment_word_map(token, klass):
                    t1 = 0.2 * maxScore
                    t2 = 0.4 * maxScore
                    t3 = 0.6 * maxScore
                    t4 = 0.8 * maxScore
                    score = self.sentimentExplanation[klass][token]
                    return discord_format(token, abs(score) > t2 and abs(score) < t3 or abs(score) > t4, abs(score) > t3, score > t1, score < -t1)
                
                if self.emoji_to_klass(str(payload.emoji)) is not None:
                    explained = ' '.join([sentiment_word_map(token, self.emoji_to_klass(str(payload.emoji))) for token in self.sentimentMessageOriginal])
                    self.sendQueue.put(explained + ' <SENTIMENT_MESSAGE>', self.sentimentMessage.channel)

            elif self.sentimentResult is not None and payload.message_id == self.sentimentResult.id:
                if str(payload.emoji) == '📊' and not self.sentimentGraphSent:
                    self.sentimentGraphSent = True
                    graph = self.graph_explanation(self.sentimentExplanation, 'POSITIVE')
                    if len(graph) > 1964:
                        graphs = self.split_graph(graph)
                        for g in graphs[:-1]:
                            self.sendQueue.put(g, self.sentimentResult.channel)
                        self.sendQueue.put('<SENTIMENT_EXPLANATION_GRAPH> ' + graphs[-1], self.sentimentResult.channel)
                    else:
                        self.sendQueue.put('<SENTIMENT_EXPLANATION_GRAPH> ' + graph, self.sentimentResult.channel)
                elif str(payload.emoji) == '💬':
                    await self.sentimentResult.edit(content=self.graph_prediction(self.sentimentPrediction))
                    
            elif self.sentimentGraph is not None and payload.message_id == self.sentimentGraph.id:
                if self.emoji_to_klass(str(payload.emoji)) is not None:
                    graph = self.graph_explanation(self.sentimentExplanation, self.emoji_to_klass(str(payload.emoji)))
                    if len(graph) > 1964:
                        graphs = self.split_graph(graph)
                        for g in graphs[:-1]:
                            self.sendQueue.put(g, self.sentimentResult.channel)
                        self.sendQueue.put('<SENTIMENT_EXPLANATION_GRAPH> ' + graphs[-1], self.sentimentResult.channel)
                    else:
                        await self.sentimentGraph.edit(content=graph)

            # Anime Class Reactions ============================================
            elif self.animeMessage is not None and payload.message_id == self.animeMessage.id:
                maxScore = 0
                for key, score in self.animeExplanation[self.emoji_to_klass(str(payload.emoji))].items():
                    if abs(score) > maxScore: maxScore = abs(score)
                def anime_word_map(token, klass):
                    t1 = 0.2 * maxScore
                    t2 = 0.4 * maxScore
                    t3 = 0.6 * maxScore
                    t4 = 0.8 * maxScore
                    score = self.animeExplanation[klass][token]
                    return discord_format(token, abs(score) > t2 and abs(score) < t3 or abs(score) > t4, abs(score) > t3, score > t1, score < -t1)

                if self.emoji_to_klass(str(payload.emoji)) is not None:
                    explained = ' '.join([anime_word_map(token, self.emoji_to_klass(str(payload.emoji))) for token in self.animeMessageOriginal])
                    self.sendQueue.put(explained + ' <ANIME_CLASS_MESSAGE>', self.animeMessage.channel)
            
            elif self.animeResult is not None and payload.message_id == self.animeResult.id:
                if str(payload.emoji) == '📊' and not self.animeGraphSent:
                    self.animeGraphSent = True
                    graph = self.graph_explanation(self.animeExplanation, 'Popular and Liked')
                    if len(graph) > 1964:
                        graphs = self.split_graph(graph)
                        for g in graphs[:-1]:
                            self.sendQueue.put(g, self.animeResult.channel)
                        self.sendQueue.put('<ANIME_CLASS_EXPLANATION_GRAPH> ' + graphs[-1], self.animeResult.channel)
                    else:
                        self.sendQueue.put('<ANIME_CLASS_EXPLANATION_GRAPH> ' + graph, self.animeResult.channel)
                elif str(payload.emoji) == '💬':
                    await self.animeResult.edit(content=self.graph_prediction(self.animePrediction))
                    
            elif self.animeGraph is not None and payload.message_id == self.animeGraph.id:
                if self.emoji_to_klass(str(payload.emoji)) is not None:
                    graph = self.graph_explanation(self.animeExplanation, self.emoji_to_klass(str(payload.emoji)))
                    if len(graph) > 1964:
                        graphs = self.split_graph(graph)
                        for g in graphs[:-1]:
                            self.sendQueue.put(g, self.animeResult.channel)
                        self.sendQueue.put('<ANIME_CLASS_EXPLANATION_GRAPH> ' + graphs[-1], self.animeResult.channel)
                    else:
                        await self.animeGraph.edit(content=graph)
                    

            # Anime Genre Reactions ============================================   
            elif self.animeGenreMessage is not None and payload.message_id == self.animeGenreMessage.id:
                maxScore = 0
                subgenre = 0 if self.emoji_to_klass(str(payload.emoji)) in ['Shounen', 'Shoujo', 'Seinen', 'Josei'] else 1 if self.emoji_to_klass(str(payload.emoji)) in ['Action', 'Romance', 'Drama', 'Comedy'] else 2
                for key, score in self.animeGenreExplanation[subgenre][self.emoji_to_klass(str(payload.emoji))].items():
                    if abs(score) > maxScore: maxScore = abs(score)
                def genre_word_map(token, klass):
                    t1 = 0.2 * maxScore
                    t2 = 0.4 * maxScore
                    t3 = 0.6 * maxScore
                    t4 = 0.8 * maxScore
                    subgenre = 0 if klass in ['Shounen', 'Shoujo', 'Seinen', 'Josei'] else 1 if klass in ['Action', 'Romance', 'Drama', 'Comedy'] else 2
                    score = self.animeGenreExplanation[subgenre][klass][token]
                    return discord_format(token, abs(score) > t2 and abs(score) < t3 or abs(score) > t4, abs(score) > t3, score > t1, score < -t1)

                if self.emoji_to_klass(str(payload.emoji)) is not None:
                    explained = ' '.join([genre_word_map(token, self.emoji_to_klass(str(payload.emoji))) for token in self.animeGenreMessageOriginal])
                    self.sendQueue.put(explained + ' <ANIME_GENRE_MESSAGE>', self.animeGenreMessage.channel)

            
            elif self.animeGenreResult is not None and payload.message_id == self.animeGenreResult.id:
                if str(payload.emoji) == '📊' and not self.animeGenreGraphSent:
                    self.animeGenreGraphSent = True
                    graph = self.graph_explanation(self.animeGenreExplanation[0], 'Josei')
                    if len(graph) > 1964:
                        graphs = self.split_graph(graph)
                        for g in graphs[:-1]:
                            self.sendQueue.put(g, self.animeGenreResult.channel)
                        self.sendQueue.put('<ANIME_GENRE_EXPLANATION_GRAPH> ' + graphs[-1], self.animeGenreResult.channel)
                    else:
                        self.sendQueue.put('<ANIME_GENRE_EXPLANATION_GRAPH> ' + graph, self.animeGenreResult.channel)
                elif str(payload.emoji) == '💬':
                    await self.animeGenreResult.edit(content=self.graph_prediction({**self.animeGenrePrediction[0], **self.animeGenrePrediction[1], **self.animeGenrePrediction[2]}))
                    
            elif self.animeGenreGraph is not None and payload.message_id == self.animeGenreGraph.id:
                if self.emoji_to_klass(str(payload.emoji)) is not None:
                    subgenre = 0 if self.emoji_to_klass(str(payload.emoji)) in ['Shounen', 'Shoujo', 'Seinen', 'Josei'] else 1 if self.emoji_to_klass(str(payload.emoji)) in ['Action', 'Romance', 'Drama', 'Comedy'] else 2
                    graph = self.graph_explanation(self.animeGenreExplanation[subgenre], self.emoji_to_klass(str(payload.emoji)))
                    if len(graph) > 1964:
                        graphs = self.split_graph(graph)
                        for g in graphs[:-1]:
                            self.sendQueue.put(g, self.animeGenreResult.channel)
                        self.sendQueue.put('<ANIME_GENRE_EXPLANATION_GRAPH> ' + graphs[-1], self.animeGenreResult.channel)
                    else:
                        await self.animeGenreGraph.edit(content=graph)

def setup(bot): bot.add_cog(ClassifyCog(bot))