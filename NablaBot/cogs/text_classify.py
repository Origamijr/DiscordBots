'''
import importlib.util
spec = importlib.util.spec_from_file_location("explain", "C:/Users/origa/OneDrive/Documents/HW/CSE 156/Final_Project/explain.py")
foo = importlib.util.module_from_spec(spec)
spec.loader.exec_module(foo)
''''''
import sys
sys.path.append('C:/Users/origa/OneDrive/Documents/HW/CSE 156/Final_Project')
from explain import Explainer

e = Explainer()
print(e.explain('this sentence'))'''