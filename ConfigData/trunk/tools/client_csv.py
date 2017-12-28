#coding:utf-8

import os
import sys
from xlrd import open_workbook,empty_cell

reload(sys) 
sys.setdefaultencoding( "utf-8" ) 

target_dir = "../csvnew"
if not os.path.exists(target_dir):
	os.mkdir(target_dir)

xlsx_file_name = sys.argv[1]

tail_period_idx = xlsx_file_name.rfind('.')
tail_slash_idx = -1
try:
	tail_slash_idx = xlsx_file_name.rfind('/')
except Exception,e:
	tail_slash_idx = -1
	
target_name = target_dir + "/" + xlsx_file_name[tail_slash_idx + 1:tail_period_idx] + ".csv"

book = open_workbook(xlsx_file_name, 'rb')

row_header = 1

skip_column= {}

def deal_sheet(table,out):
	if table.nrows <= 0 or table.ncols <= 0 :
		print "empty sheet : " + table.name + " in(" + xlsx_file_name + ")"
		sys.exit(1)
	cells = table.row(row_header)
	
	cur_column = 0
	for cell in cells:
		if cell.value.lstrip().startswith('#'):
			skip_column[cur_column] = True
			cur_column = cur_column + 1
			continue
		cur_column = cur_column + 1
			
	cells = table.row(row_header)
	first = True
	#这是列头
	cur_column = 0
	for cell in cells:
		if skip_column.get(cur_column):
			cur_column = cur_column  + 1
			continue
			
		cur_column = cur_column  + 1
		
		if first:
			first = False
		else:
			out.write("`")
			
		out.write(cell.value)
	out.write("\n")
	#具体的数据
	
	front = True
	for i in range(row_header + 1,table.nrows ):
		if front:
			front = False
		else:
			out.write("\n")
			
		cells = table.row(i)
		first = True
		
		#当遇到有空行的情况就算结束
		if len(str(cells[0].value)) == 0:
			break
		cur_column = 0
		for cell in cells:
			if skip_column.get(cur_column):
				cur_column = cur_column  + 1
				continue
			cur_column = cur_column  + 1
			
			if first:
				first = False
			else:
				out.write("`")
			if not isinstance(cell.value,float):
				out.write(unicode(cell.value))
			else:
				real_value = int(cell.value)
				if real_value == cell.value:
					out.write(unicode((int(cell.value))))
				else:
					out.write(unicode(cell.value))
				
fcsv = open(target_name,'w+')	
print(target_name)
deal_sheet(book.sheets()[0],fcsv)
