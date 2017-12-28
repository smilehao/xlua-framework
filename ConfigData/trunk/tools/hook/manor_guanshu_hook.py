import os
import sys

def _hook(name,value,out):
	if name == 'unlock_building':
		if len(value) <= 0 :
			out.write("{}")
			return True
		temp_array = value.split('|')
		out.write("{");
		for i in range(len(temp_array)):
			out.write(temp_array[i] + ",")
		out.write("}")
		return True
	return False