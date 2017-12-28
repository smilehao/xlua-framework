#!/usr/bin/env python
#coding:utf-8

import os
import sys
from xlrd import open_workbook,empty_cell

tmp_file_path="temp/"
file_prefix="lua_"
front_file_prefix="cs_"
svr_proto_exec = 0
front_proto_exec = 0
client_dir = "../pbconfig/"
client_dat = "../pbconfig_data/"
server_dir = "sconfig/"
server_dat = "sdata/"
server_hook_file="hook/"
if not os.path.exists(client_dir):
	os.mkdir(client_dir)
if not os.path.exists(server_dir):
	os.mkdir(server_dir)
if not os.path.exists(server_dat):
	os.mkdir(server_dat)
if not os.path.exists(tmp_file_path):
	os.mkdir(tmp_file_path)

if not os.path.exists(server_hook_file):
	os.mkdir(server_hook_file)

def get_str_between(str, begin, end):
	if len(str) == 0:
		sys.exit(1)

	if len(begin) == 0 or str.find(begin) == -1:
		begin_pos = 0
	else:
		begin_pos = str.find(begin) + len(begin);

	if len(begin) == 0 or str.find(end) == -1:
		end_pos = len(str)
	else:
		end_pos = str[begin_pos:].find(end) + begin_pos

	return str[begin_pos:end_pos]

def get_base_name(realName):
	realName = realName[realName.rfind('/') + 1:]
	realName = realName[0:realName.rfind('.') ]
	return realName
	
if len(sys.argv) != 2 :
	print "input xlsx file name"
	sys.exit(1)

xlsx_file_name = sys.argv[1]
base_name = get_base_name(xlsx_file_name)
book = open_workbook(xlsx_file_name, 'rb')

#cell.ctype等于0表示空 等于1表示文本 空格属于文本 等于2表示数值
get_int_func="\nimport sys\n"
get_int_func+="\ndef get_int(check_cell, rown, coln):\n"
get_int_func+="\tif 0 == check_cell.ctype:\n"
get_int_func+="\t\treturn 0\n"
get_int_func+="\telif 1 == check_cell.ctype:\n"
get_int_func+="\t\tvalue_str = \"%s\" % unicode(check_cell.value)\n"
get_int_func+="\t\tif value_str.isdigit():\n"
get_int_func+="\t\t\treturn int(check_cell.value)\n"
get_int_func+="\t\telif len(value_str) == 0:\n"
get_int_func+="\t\t\treturn 0\n"
get_int_func+="\t\telse:\n"
get_int_func+="\t\t\tprint \"cell(%d %d) \" %(rown,coln) + \"value \\\"\" + value_str + \"\\\" error!\"\n"
get_int_func+="\t\t\tsys.exit(1)\n"
get_int_func+="\telse:\n"
get_int_func+="\t\treturn int(check_cell.value)\n\n"

get_str_func="\ndef get_str(check_cell, rown, coln):\n"
get_str_func+="\tif 0 == check_cell.ctype:\n"
get_str_func+="\t\treturn \'\'\n"
get_str_func+="\telif 2 == check_cell.ctype:\n"
get_str_func+="\t\treturn unicode(int(check_cell.value))\n"
get_str_func+="\telse:\n"
get_str_func+="\t\treturn unicode(check_cell.value)\n\n"

def deal_sheet(index, table):
	tmp_index = table.name.find("#")
	if tmp_index == -1 :
		return 0

	if table.nrows <= 0 or table.ncols <= 0 :
		print "empty sheet : " + table.name
		sys.exit(1)

	tmp_name = table.name[tmp_index + 1:]
	name_info = tmp_name.split('#')

	svr_name = ""
	front_name = ""

	global svr_proto_exec
	global front_proto_exec

	if len(name_info) == 1 and name_info[0] != "":
		svr_name = name_info[0]
		svr_proto_exec = 1
		front_name = name_info[0]
		front_proto_exec = 1

	if len(name_info) == 2:
		if name_info[0] != "":
			svr_name = name_info[0]
			svr_proto_exec = 1
		if name_info[1] != "":
			front_name = name_info[1]
			front_proto_exec = 1

	if svr_name != "" and svr_name == base_name :
		print "sheet name : " + svr_name + " same with xlsx name"
		sys.exit(1)

	if front_name != "" and front_name == base_name :
		print "sheet name : " + front_name + " same with xlsx name"
		sys.exit(1)

	if svr_name != "" :
		pb_file.write("message " + file_prefix + svr_name + "_unit " + "{\n");
		py_file.write("table = book.sheet_by_index(%d)\n" % index );
		py_file.write(svr_name + " = " + base_name + "." + svr_name + "_rows\n");
		py_file.write(get_int_func);
		py_file.write(get_str_func);
		#py_file.write("for row_index in range (1, table.nrows):\n"); #第0行用作标识定义 第1行是数据开始
		py_file.write("for row_index in range (2, table.nrows):\n"); #第0行用作标识定义 第1行是前端标识定义 第2行是数据开始
		py_file.write("\tif len(str(table.cell(row_index, 0).value)) == 0:\n\t\tbreak\n")
		py_file.write("\t" + svr_name + "_unit = " + base_name + "." + svr_name + "_rows.add()\n");
	
	if front_name != "" :
		pb_front_file.write("message " + front_file_prefix + front_name + "_unit " + "{\n");
		py_front_file.write("table = book.sheet_by_index(%d)\n" % index );
		py_front_file.write(front_name + " = " + base_name + "." + front_name + "_rows\n");
		py_front_file.write(get_int_func);
		py_front_file.write(get_str_func);
		#py_front_file.write("for row_index in range (1, table.nrows):\n"); #第0行用作标识定义 第1行是数据开始
		py_front_file.write("for row_index in range (2, table.nrows):\n"); #第0行用作标识定义 第1行是前端标识定义 第2行是数据开始
		py_front_file.write("\t" + front_name + "_unit = " + base_name + "." + front_name + "_rows.add()\n");

	cells = table.row(0)
	i = 0
	pb_col_index = 0
	pb_col_index_front = 0
	for cell in cells:
		if cell.value.find("#") == -1 :
			i = i + 1
			continue;

		tmp = cell.value[cell.value.find("#") + 1:]
		col_info = tmp.split(':')
		
		if len(col_info) != 3:
			i = i + 1
			continue;
		
		front_col_name = col_info[0];
		server_col_name = col_info[1];
		col_type = col_info[2];
		
		if col_type == "":
			i = i + 1
			continue;
		
		s = "\t optional ";
		s_front = "\t optional ";
		
		s2 = "\t" + svr_name + "_unit." + server_col_name + " = ";
		s2_front = "\t" + front_name + "_unit." + front_col_name + " = ";
		
		if col_type == "int":
			s += "int32 "
			s_front += "int32 "
			s2 += "get_int"
			s2_front += "get_int";
		else :
			s += "string "
			s_front += "string "
			s2 += "get_str"
			s2_front += "get_str"
		
		if svr_name != "" and server_col_name != "" and server_col_name.lower() != "none":
			s += server_col_name + " = %d ;\n" % (pb_col_index + 1)
			s2 += "(table.cell(row_index, %d" %i + "), row_index, %d)\n" %i;
			pb_file.write(s);
			py_file.write(s2);
			pb_col_index += 1
		
		if front_name != "" and front_col_name != "" and front_col_name.lower() != "none":
			s_front += front_col_name + " = %d ;\n" % (pb_col_index_front + 1)
			s2_front += "(table.cell(row_index, %d" %i + "), row_index, %d)\n" %i;
			pb_front_file.write(s_front);
			py_front_file.write(s2_front);
			pb_col_index_front += 1
		i = i + 1

	if svr_name != "" :
		pb_file.write("}\nrepeated " + file_prefix + svr_name + "_unit " + svr_name  + "_rows = %d;\n\n" % (index + 1));
		pb_file.flush();
	
	if front_name != "" :
		pb_front_file.write("}\nrepeated " + front_file_prefix + front_name + "_unit " + front_name  + "_rows = %d;\n\n" % (index + 1));
		pb_front_file.flush();

pb_file_name = tmp_file_path + file_prefix + base_name + ".proto"
pb_file = open(pb_file_name, 'w+');

pb_front_file_name = tmp_file_path+ front_file_prefix + base_name + ".proto"
pb_front_file = open(pb_front_file_name, 'w+');

pb_file.write("message " + file_prefix + base_name + "{\n\n");
pb_front_file.write("message " + front_file_prefix + base_name + "{\n\n");

py_file_name = tmp_file_path+ file_prefix + "create_" + base_name + ".py"
py_front_file_name = tmp_file_path+ front_file_prefix + "create_" + base_name + ".py"

py_file = open(py_file_name, 'w+');
py_front_file = open(py_front_file_name, 'w+');

py_file.write("from xlrd import open_workbook,empty_cell\n");
py_file.write("from google.protobuf import descriptor\n");
py_file.write("import " +  file_prefix + base_name + "_pb2\n");
py_file.write("from tolua import toluaaaa\n");
py_file.write("import sys\n")
py_file.write("import os\n")
py_file.write("book = open_workbook('" + xlsx_file_name +"','rb')\n");
py_file.write(base_name + " = " + file_prefix + base_name + "_pb2." + file_prefix + base_name + "()\n");

py_front_file.write("from xlrd import open_workbook\n");
py_front_file.write("import " + front_file_prefix + base_name + "_pb2\n");
py_front_file.write("book = open_workbook('" + xlsx_file_name +"','rb')\n");
py_front_file.write(base_name + " = " + front_file_prefix + base_name + "_pb2." + front_file_prefix + base_name + "()\n");

index = 0
for table in book.sheets():
	deal_sheet(index, table)
	index += 1

pb_file.write("}\n");
pb_file.flush();

pb_front_file.write("}\n");
pb_front_file.flush();


if svr_proto_exec >= 0 :
	ret_value = os.system("protoc " + pb_file_name + " --python_out=./");
	if ret_value != 0:
		print base_name + ".xlsx " + pb_file_name + " Error(%d)!" % ret_value
		sys.exit(1)

if front_proto_exec >= 0 :
	ret_value = os.system("protoc " + pb_front_file_name + " --python_out=./");
	ret_value = os.system("protoc " + pb_front_file_name + " --python_out=./");
	if ret_value != 0:
		print base_name + ".xlsx " + pb_front_file_name + " Error(%d)!" % ret_value
		sys.exit(1)


py_file.write("str = " + base_name + ".SerializeToString()\n");
py_file.write("fileHandle = open('" +server_dat +  file_prefix + base_name + ".dat', 'wb+')\n");
py_file.write("fileHandle.write(str)\n");
py_file.write("fileHandle.close()\n");
py_file.write("fileHandle = open('" +server_dir +  file_prefix + base_name + ".lua', 'wb+')\n");
py_file.write("col_hook = None\n")
py_file.write("sys.path.append(\"" + server_hook_file + "\")\n")
py_file.write("if os.path.exists(\"" + server_hook_file  + base_name + "_hook.py\"):\n")
py_file.write("\tfrom " + base_name + "_hook import _hook\n")
py_file.write("\tcol_hook=_hook\n")
py_file.write("toluaaaa("+base_name + ",fileHandle,col_hook)\n");
py_file.flush();

py_front_file.write("str = " + base_name + ".SerializeToString()\n");
py_front_file.write("fileHandle = open('" +client_dat + front_file_prefix + base_name + ".dat', 'wb+')\n");
py_front_file.write("fileHandle.write(str)\n");
py_front_file.write("fileHandle.close()\n");
py_front_file.flush();

if svr_proto_exec >= 0 :
	ret_value = os.system("python " + py_file_name);
	if ret_value != 0:
		print base_name + ".xlsx " + py_file_name + " Error(%d)!" % ret_value
		sys.exit(1)

if front_proto_exec >= 0 :
	ret_value = os.system("python " + py_front_file_name);
	if ret_value != 0:
		print base_name + ".xlsx " + py_front_file_name + " Error(%d)!" % ret_value
		sys.exit(1)

py_file.close()
py_front_file.close()

#生成cs文件
'''
pb_front_file.close()
front_cs_file=client_dir + front_file_prefix + base_name + ".cs"
#final_client_path = client_dir + front_file_prefix + base_name + ".proto"
#os.remove(final_client_path)
#shutil.move(pb_front_file_name,final_client_path )
os.system("protogen.exe -i:" + pb_front_file_name + " -o:" + front_cs_file + " -ns:SanguoConfig");
'''
#生成服务器文件

print "\t" + base_name + ".xlsx\tOK!"

