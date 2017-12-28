# coding: UTF-8

import sys
import os
import subprocess

os.chdir(os.path.dirname(sys.argv[0]))
rela_dir = "../"
all_files = os.listdir(rela_dir)

all_excel = []
for _o in all_files:
	if not _o.endswith(".xlsx"):
		continue
	if _o.startswith("~"):
		continue
	all_excel.append(_o)
	
lua_config_path = "sconfig"
__import__('shutil').rmtree(lua_config_path)
if not os.path.exists(lua_config_path):
	os.mkdir(lua_config_path)

for _o in all_excel:
	print _o
	try:
		p = subprocess.Popen(['python','new_transform.py',rela_dir + _o, lua_config_path])
		print 'translate (%s)' %  _o,
		exit_code = p.wait()
		if exit_code == 0:
			print('\tDONE!')
		else:
			print('Failed,ExitCode ' + str(exit_code))
	except Exception,e:
		print Exception,e
		#os.system("pause")

#p = subprocess.Popen(['LuaEx.exe','-i',"checker\config_mng.lua"])
#print("CHECKING NOW>>>")

#exit_code = p.wait()
#if exit_code == 0:
print("============SUCCEEDED!=================")
#os.system("pause")
exit(0)