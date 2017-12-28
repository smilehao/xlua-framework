import sys
import os
import shutil

rela_dir = "sconfig"
all_files = os.listdir(rela_dir)

all_excel = []
for _o in all_files:
	if not _o.endswith(".lua"):
		continue
	if _o.startswith("~"):
		continue
	src = rela_dir + '/' + _o
	print(src)
	shutil.copy( src ,'../../../server/trunk/sanguo/scripts/conf/')
