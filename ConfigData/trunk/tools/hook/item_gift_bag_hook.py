import os
import sys

def _hook(name,value,out):
	if name == 'extend_item_id_list':
		if len(value) <= 0 :
			out.write("{}")
			return True
		temp_array = value.split('|')
		out.write("{");
		for i in range(len(temp_array)):
			out.write(temp_array[i] + ",")
		out.write("}")
		return True
	if name == 'extend_item_count_list':
		if len(value) <= 0 :
			out.write("{}")
			return True
		temp_array = value.split('|')
		out.write("{");
		for i in range(len(temp_array)):
			out.write(temp_array[i] + ",")
		out.write("}")
		return True
	if name == 'extend_prob_list':
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