
#coding:utf-8

import os
import sys
import xlrd
import traceback

reload(sys)
sys.setdefaultencoding('utf-8')

xlsx_file_name = None

def get_base_name(realName):
	realName = realName[realName.rfind('/') + 1:]
	realName = realName[0:realName.rfind('.') ]
	return realName
	
def get_int(value):
	value = str(value)
	if len(value) == 0:
		value = "0"
	return int(float(value))
	
def get_float(value):
	value = str(value)
	if len(value) == 0:
		value = "0.0"
	return float(value)
	
def int_type_handle(s,data,field_info):
	field_name = field_info[0]
	field_index = field_info[2]

	s.write('%s = %d,\n' % (field_name,get_int(data[field_index].value)))
	
def string_type_handle(s,data,field_info):
	field_name = field_info[0]
	field_index = field_info[2]
	value = str(data[field_index].value)
	if len(value) == 0:
		value = ""
	value = unicode(value).encode('utf-8')
	field_name = unicode(field_name).encode('utf-8')
	#print(field_name)
	s.write("%s = \"%s\",\n" % (field_name,str(value).replace('\n','\\n').replace('"', '\\"')))

def float_type_handle(s,data,field_info):
	field_name = field_info[0]
	field_index = field_info[2]
	s.write('%s = %.2f,\n' % (field_name,get_float(data[field_index].value)))

def int_array_type_handle(s,data,field_info):
	field_name = field_info[0]
	field_index = field_info[2]
	str_list = str(data[field_index].value)
	s.write('%s = ' % field_name)
	if len(str_list) == 0:
		s.write('{},\n')
		return
		
	s.write("{")
	int_list = str_list.split('|')
	for int_value in int_list:
		s.write(str(get_int(int_value)) + ',')
	s.write('},\n')
	
TYPE_MAP = {
	'int': int_type_handle,
	'string' : string_type_handle,
	'float' : float_type_handle,
	'int_array' : int_array_type_handle,
}

def get_field_type(type_name):
	return TYPE_MAP.get(type_name)


##收集所有需要输出的字段
def collect_all_fields(table):
	if table.nrows <= 0 or table.ncols <= 0 :
		print("[%s] empty sheet" % xlsx_file_name)
		exit(1)
	files = []
	cells = table.row(1)
	field_index = 0
	for column_info in cells:
		info_list = column_info.value.split('#')
		#print info_list
		if len(info_list) != 2:
			field_index += 1
			continue
			
		fields_info = info_list[1].split(':')
		field_name = None
		field_type = None
		if len(fields_info) > 3:
			print('[%s] invalid fields_info (%s)' % (xlsx_file_name ,info_list[1]))
			exit(3)
		elif len(fields_info) > 2:
			field_name = fields_info[1]
			field_type = get_field_type(fields_info[2])
		elif len(fields_info) > 1:
			field_name = fields_info[0]
			field_type = get_field_type(fields_info[1])
		else:
			print('[%s] unknown fields_info (%s) ' % (xlsx_file_name ,info_list[1]))
			exit(4)
			
		if not field_name or not field_type:
			print('[%s] invalid fields_info(%s) field_name(%s),field_type(%s)' % (xlsx_file_name,fields_info[1],field_name,field_type))
			exit(5)
			
		if field_name.upper() == "NONE":
			field_index += 1
			continue
			
		files.append((field_name,field_type,field_index))
		field_index += 1
	return files

	
class StringStream:
	def __init__(self,str):
		self.str = []
	
	def write(self,str):
		self.str.append(str)
	
	def tostring(self):
		return ''.join(self.str)

def create_lua_string(table,exported_fields):
	one_lua_struct = StringStream("")
	duplicated_id = {}
	#从第3列开始	
	one_lua_struct.write('local config = {\n')
	for i in xrange(2,table.nrows):
		cells = table.row(i)
		
		if len(str(cells[0].value)) == 0:
			break
		for field_info in exported_fields:
			if field_info[0] == "id":
				id_val = get_int(cells[field_info[2]].value)
				one_lua_struct.write('[%s] = {\n' % id_val)
				if duplicated_id.has_key(id_val):
					#判断ID是否重复
					print("detected the duplicated id %d" % id_val)
					exit(20)
				duplicated_id[id_val] = True
					
			type_handler = field_info[1]
			type_handler(one_lua_struct,cells,field_info)
			
		one_lua_struct.write('},\n')
		#print('handling ...',get_int(str(cells[0].value)))
		
	one_lua_struct.write('}\nreturn config')
	return one_lua_struct
	

def translate():
	try:	
		if len(sys.argv) < 3:
			print('input xlsx required')
			exit(100)
		global xlsx_file_name
		
		xlsx_file_name = sys.argv[1]
		output_path = sys.argv[2]
								
		book = xlrd.open_workbook(xlsx_file_name, 'rb')
		table = book.sheet_by_index(0)
		if len(table.name.split('#')) < 2:
			#这个表无需转
			exit(0)
			
		realName = get_base_name(xlsx_file_name)

		exported_lua_name = realName + ".lua"

		exported_fields = collect_all_fields(book.sheet_by_index(0))
		if(exported_fields[0][0] != "id"):
			print('[%s] invalid id fields_info (%s)' % (xlsx_file_name ,exported_fields[0][0]))
			exit(10)
			
		#print(exported_fields)

		exported_lua_string = create_lua_string(book.sheet_by_index(0),exported_fields)
		output_file = open( '%s/%s' % (output_path,exported_lua_name),'w+')
		output_file.write(exported_lua_string.tostring())
		output_file.close()
		exit(0)
		#print(exported_lua_string.tostring())
	except Exception,e:
		print("[%s] There are some problems in the xlsx,check it carefully please" % xlsx_file_name)
		print(e)
		traceback.print_exc(file=sys.stdout)
		exit(20)


translate()	