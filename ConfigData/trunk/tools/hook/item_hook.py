import os
import sys

def _hook(name,value,out):
	if name == 'drop':
		if len(value) <= 0 :
			out.write("{}")
			return True
		all_drop_item = value.split('|')
		out.write("{");
		for i in range(len(all_drop_item)):
			out.write(all_drop_item[i] + ",")
		out.write("}")
		return True
	return False