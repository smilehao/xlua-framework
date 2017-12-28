import sys
import os

rela_dir = "../"
tmp_file_path="temp/"
all_files = os.listdir(rela_dir)

all_excel = []
for _o in all_files:
	if not _o.endswith(".xlsx"):
		continue
	if _o.startswith("~"):
		continue
	all_excel.append(_o)

if not os.path.exists(tmp_file_path):
	os.mkdir(tmp_file_path)
	
os.system("copy tolua.py temp /Y");

for _o in all_excel:
	os.system("trans_xlsx.py " + rela_dir + _o)

#os.system("gen_loadfile.py")
#os.system("pause")
