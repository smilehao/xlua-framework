import os
import sys

def _hook(name,value,out):
	if name == 'extra_drop_list':
		if len(value) <= 0 :
			out.write("{}")
			return True
		temp_array = value.split('|')
		out.write("{");
		for i in range(len(temp_array)):
			out.write(temp_array[i] + ",")
		out.write("}")
		return True
	elif name == 'open_copy_list':
		if len(value) <= 0 :
			out.write("{}")
			return True
		temp_array = value.split('|')
		out.write("{");
		for i in range(len(temp_array)):
			out.write(temp_array[i] + ",")
		out.write("}")
		return True
	elif name == 'prev_copy_list':
		if len(value) <= 0 :
			out.write("{}")
			return True
		temp_array = value.split('|')
		out.write("{");
		for i in range(len(temp_array)):
			out.write(temp_array[i] + ",")
		out.write("}")
		return True		
	elif name == 'monster_group':
		if len(value) <= 0 :
			out.write("{}")
			return True
		all_arrays = value.split('|')
		out.write("{");
		for i in range(len(all_arrays)):
			all_objs = all_arrays[i].split(':')
			out.write("{monster_id = " + all_objs[0] + ", born_point = " + all_objs[1] + " },")
		out.write("}")
		return True
	return False	
	return False