import os
import sys

def _hook(name,value,out):
	if name == 'skill_list':
		if len(value) <= 0 :
			out.write("{}")
			return True
		all_arrays = value.split('|')
		out.write("{");
		for i in range(len(all_arrays)):
			out.write(all_arrays[i] + ",")
		out.write("}")
		return True
	return False