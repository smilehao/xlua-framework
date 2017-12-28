import sys
import os

rela_dir = "../"
all_files = os.listdir(rela_dir)

all_excel = []
for _o in all_files:
	if not _o.endswith(".xlsx"):
		continue
	if _o.startswith("~"):
		continue
	if _o == "arena_robot.xlsx":
		continue
	all_excel.append(_o)

	
os.system("copy tolua.py temp /Y");

for _o in all_excel:
	os.system("client_csv.py " + rela_dir + _o)

#os.system("gen_loadfile.py")
#os.system("pause")
print("DONE")
