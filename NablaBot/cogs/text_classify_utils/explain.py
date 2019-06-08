from .sentiment import get_sentiment_classifier
from .anime_classify import get_anime_classifier
from sklearn.pipeline import make_pipeline
from lime.lime_text import LimeTextExplainer
import warnings
warnings.simplefilter(action='ignore', category=FutureWarning)

class Explainer:

    def __init__(self, data="sentiment"):
        vectorizer, linreg, self.classes = get_sentiment_classifier() if data == "sentiment" else (get_anime_classifier('popularity') if data == "anime_popularity"
                                        else (get_anime_classifier('audience') if data == "anime_audience"
                                        else (get_anime_classifier('theme') if data == "anime_theme"
                                        else (get_anime_classifier('setting') if data == "anime_setting"
                                        else None))))
        self.classify_pipeline = make_pipeline(vectorizer, linreg)
        self.explainer = LimeTextExplainer(class_names=self.classes, split_expression=u' +')

    def predict(self, text):
        return {klass: prob for klass, prob in zip(self.classes, self.classify_pipeline.predict_proba([text]).tolist()[0])}

    def explain(self, text):
        exp = self.explainer.explain_instance(text, self.classify_pipeline.predict_proba, num_features=2*len(text), labels=[i for i in range(0,len(self.classes))])
        return {self.classes[i]: dict(exp.as_list(label=i)) for i in range(0,len(self.classes))}

if __name__ == "__main__":
    e = Explainer()
    examples = ['This is a bad sentence. I hate it.',
                'This is a good sentece. Highly recommended!']
    for eg in examples:
        print(repr(e.predict(eg)) + ' - ' + repr(e.explain(eg)))