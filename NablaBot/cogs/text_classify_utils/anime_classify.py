import warnings
warnings.simplefilter(action='ignore', category=FutureWarning)

def read_file(filename, classifier=None, logging=True, vectorizer='count'):
    creating = False
    if classifier is None:        
        class Data: pass
        classifier = Data()
        creating = True

        if logging: print("-- building vectorizer")
        from sklearn.feature_extraction.text import CountVectorizer, TfidfVectorizer
        import sys
        if vectorizer == 'count':
            pass
            # classifier.count_vect = CountVectorizer(tokenizer=my_tokenizer, min_df=5)
        elif vectorizer == 'tfidf':
            classifier.count_vect = TfidfVectorizer(tokenizer=my_tokenizer, norm='l2', min_df=5)

    if logging: print("-- train data")
    classifier.train_data, classifier.train_labels = read_tsv(filename, logging)
    if logging: print(len(classifier.train_data))

    if logging: print("-- transforming data and labels")
    if creating:
        classifier.trainX = classifier.count_vect.fit_transform(classifier.train_data)
    else:
        classifier.trainX = classifier.count_vect.transform(classifier.train_data)

    from sklearn import preprocessing
    classifier.le = preprocessing.LabelEncoder()
    classifier.le.fit(classifier.train_labels)
    classifier.target_labels = classifier.le.classes_
    classifier.trainy = classifier.le.transform(classifier.train_labels)
    
    return classifier

def my_tokenizer(text):
    from nltk.tokenize import TweetTokenizer
    tokens = TweetTokenizer(reduce_len=True).tokenize(text)

    return tokens

def read_tsv(fname, logging=True):
    data = []
    labels = []
    with open(fname, 'r', encoding="utf-8") as f:
        for line in f:
            (label,text) = line.strip().split("\t")
            labels.append(label)
            data.append(text)

    return data, labels

def get_anime_classifier(data='popularity'):
    classifier = read_file('runtime_db/NablaBot/anime/' + data + '_labels.tsv', logging=False, vectorizer='tfidf')
    from .classify import train_classifier
    cls = train_classifier(classifier.trainX, classifier.trainy)
    return classifier.count_vect, cls, classifier.target_labels

if __name__ == "__main__":
    print("Reading data")
    classifier = read_file('runtime_db/NablaBot/popularity_labels.tsv', vectorizer='tfidf')
    print("\nTraining classifier")
    import classify
    cls = classify.train_classifier(classifier.trainX, classifier.trainy)
    print("\nEvaluating")
    classify.evaluate(classifier.trainX, classifier.trainy, cls, classifier.train_data, classifier.target_labels, 'train')

    
    print("\nReading data")
    classifier = read_file('runtime_db/NablaBot/audience_labels.tsv', vectorizer='tfidf')
    print("\nTraining classifier")
    import classify
    cls = classify.train_classifier(classifier.trainX, classifier.trainy)
    print("\nEvaluating")
    classify.evaluate(classifier.trainX, classifier.trainy, cls, classifier.train_data, classifier.target_labels, 'train')
    
    print("\nReading data")
    classifier = read_file('runtime_db/NablaBot/theme_labels.tsv', vectorizer='tfidf')
    print("\nTraining classifier")
    import classify
    cls = classify.train_classifier(classifier.trainX, classifier.trainy)
    print("\nEvaluating")
    classify.evaluate(classifier.trainX, classifier.trainy, cls, classifier.train_data, classifier.target_labels, 'train')
    
    print("\nReading data")
    classifier = read_file('runtime_db/NablaBot/anime/setting_labels.tsv', vectorizer='tfidf')
    print("\nTraining classifier")
    import classify
    cls = classify.train_classifier(classifier.trainX, classifier.trainy)
    print("\nEvaluating")
    classify.evaluate(classifier.trainX, classifier.trainy, cls, classifier.train_data, classifier.target_labels, 'train')