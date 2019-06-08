#!/bin/python

def train_classifier(X, y):
	"""Train a classifier using the given training data.

	Trains logistic regression on the input data with default parameters.
	"""
	from sklearn.linear_model import LogisticRegression
	cls = LogisticRegression(random_state=0, solver='lbfgs', max_iter=10000)
	cls.fit(X, y)
	return cls

def evaluate(X, yt, cls, data, labels, name='data'):
	"""Evaluated a classifier on the given labeled data using accuracy."""
	from sklearn import metrics
	yp = cls.predict(X)

	import random
	misses = []
	for i in range(0,len(yt)):
		if yp[i] != yt[i]:
			misses += [(yt[i], yp[i], i)]
	print("\nExamples of missed sentences:")
	for i in random.sample(range(0,len(misses)), min(10, len(misses))):
		print('Expected ' + repr(labels[misses[i][0]]) + ' Got ' + repr(labels[misses[i][1]]) + "\t" + data[misses[i][2]])
	print("")

	acc = metrics.accuracy_score(yt, yp)
	print("  Accuracy on %s  is: %s" % (name, acc))
