import csv
import string
import re
from nltk import tokenize

csvArr = []
csvReader = csv.DictReader(open('data/anime/anime.csv', encoding="utf8"))
for row in csvReader:
    csvArr.append(row)

print(str(len(csvArr)) + ' raw entries')

def trim_synopsis(synopsis, num_sentences=0):
    synopsis = re.sub('\s+', ' ', synopsis).strip()
    if synopsis[-1] == ']':
        synopsis = synopsis[0:synopsis.rfind('[')]
    if synopsis[-1] == ')' and synopsis.find('(Source') > 0:
        synopsis = synopsis[0:synopsis.rfind('(Source')]
    if num_sentences == 0:
        return [synopsis]
    else:
        sentences = tokenize.sent_tokenize(synopsis)
        if len(sentences) < num_sentences:
            return [synopsis]
        else:
            synsplit = []
            for i in range(0, len(sentences) - num_sentences + 1):
                synsplit.append(' '.join(sentences[i:i+num_sentences]))
            return synsplit

def bucketter(bucket_f, num_sentences=0):
    bucketted = dict()
    for entry in csvArr:
        bucket = bucket_f(entry)
        if bucket is not None:
            if bucket not in bucketted:
                bucketted[bucket] = []
            for synopsis in trim_synopsis(entry[' synopsis'], num_sentences):
                bucketted[bucket].append({'synopsis': synopsis, 'name': entry[' name']})
    return bucketted

def write_bucket(buckets, filename):
    with open(filename, "w", encoding="utf-8") as f:
        for bucket, content in buckets.items():
            for entry in content:
                f.write(bucket + '\t' + entry['synopsis'] + '\n')
        f.close()

################################################################################
'''
print("\nSorting by score...")

def bucket_score(entry):
    try:
        score = float(entry[' score'])
        synopsis = entry[' synopsis']
        if score is None or synopsis is '': return None
        elif score >= 8: return '8-10'
        elif score >= 7: return '7-8'
        elif score >= 6: return '6-7'
        elif score >= 4: return '4-6'
        elif score >= 2: return '0-4'
        else: return None
    except:
        return None

bucketted = bucketter(bucket_score, 3)

for bucket, content in bucketted.items():
    print(bucket + ' ' + str(len(content)))

print("Writing score labels...")

write_bucket(bucketted, "data/anime/score_labels.tsv")
'''
################################################################################
'''
print("\nSorting by source...")

def bucket_source(entry):
    try:
        source = entry[' source']
        synopsis = entry[' synopsis']
        if source is '' or synopsis is '': return None
        if source == 'Original': return 'Original'
        if source in ['Manga', 'Web manga', '4-koma manga', 'Digital manga']: return 'Manga'
        if source in ['Light novel', 'Novel', 'Book', 'Picture book']: return 'Novel'
        if source in ['Visual novel', 'Game', 'Card game']: return 'Game'
        if source == 'Unknown': return None
        else: return 'Other'
    except:
        return None

bucketted = bucketter(bucket_source, 0)

for bucket, content in bucketted.items():
    print(bucket + ' ' + str(len(content)))

print("Writing source labels...")

write_bucket(bucketted, 'data/anime/source_labels.tsv')
'''
################################################################################

print("\nSorting by popularity...")

def bucket_popularity(entry):
    try:
        score = float(entry[' score'])
        members = int(entry[' members'])
        synopsis = entry[' synopsis']
        if score is None or members is None or synopsis is '': return None
        elif score > 7 and members > 20000: return 'Popular and Liked'
        elif score > 7: return 'not Popular but Liked'
        elif members > 5000: return 'Popular but not Liked'
        else: return 'neither Popular nor Liked'
    except:
        return None

bucketted = bucketter(bucket_popularity, 3)

for bucket, content in bucketted.items():
    print(bucket + ' ' + str(len(content)))

print("Writing popularity labels...")

write_bucket(bucketted, 'data/anime/popularity_labels.tsv')

################################################################################

print("\nSorting by audience...")

def bucket_audience(entry):
    try:
        genres = entry[' genre']
        synopsis = entry[' synopsis']
        if genres is None or synopsis is '': return None
        elif 'Josei' in genres: return 'Josei'
        elif 'Seinen' in genres: return 'Seinen'
        elif 'Shoujo' in genres: return 'Shoujo'
        elif 'Shounen' in genres: return 'Shounen'
        else: return None
    except:
        return None

bucketted = bucketter(bucket_audience, 1)

for bucket, content in bucketted.items():
    print(bucket + ' ' + str(len(content)))

print("Writing audience labels...")

write_bucket(bucketted, 'data/anime/audience_labels.tsv')

################################################################################

print("\nSorting by theme...")

def bucket_theme(entry):
    try:
        genres = entry[' genre']
        synopsis = entry[' synopsis']
        if genres is None or synopsis is '': return None
        elif 'Action' in genres: return 'Action'
        elif 'Romance' in genres: return 'Romance'
        elif 'Drama' in genres: return 'Drama'
        elif 'Comedy' in genres: return 'Comedy'
        else: return None
    except:
        return None

bucketted = bucketter(bucket_theme, 3)

for bucket, content in bucketted.items():
    print(bucket + ' ' + str(len(content)))

print("Writing theme labels...")

write_bucket(bucketted, 'data/anime/theme_labels.tsv')

################################################################################

print("\nSorting by setting...")

def bucket_setting(entry):
    try:
        genres = entry[' genre']
        synopsis = entry[' synopsis']
        if genres is None or synopsis is '': return None
        elif 'Slice of Life' in genres: return 'Slice of Life'
        elif 'Fantasy' in genres or 'Magic' in genres: return 'Fantasy'
        elif 'Supernatural' in genres: return 'Supernatural'
        elif 'Sci-Fi' in genres: return 'Sci-Fi'
        else: return None
    except:
        return None

bucketted = bucketter(bucket_setting, 3)

for bucket, content in bucketted.items():
    print(bucket + ' ' + str(len(content)))

print("Writing setting labels...")

write_bucket(bucketted, 'data/anime/setting_labels.tsv')

'''
Shounen
Shoujo
Josei
Seinen

Action
Romance
Drama
Comedy

Sol
Scifi
Fantasy (Magic)
supernatural


minscore = 10
minanime = ''
for entry in csvArr:
    try:
        if int(entry[' premiered'].split(' ')[-1]) > 2005:
            if float(entry[' score']) < minscore:
                minscore = float(entry[' score'])
                minanime = entry[' name']
    except:
        continue

print(minanime + ' ' + str(minscore))
'''