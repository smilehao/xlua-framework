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
	
lua_config_path = "LuaConfigs"
if not os.path.exists(lua_config_path):
	os.mkdir(lua_config_path)

for _o in all_excel:
	while True:
		p = subprocess.Popen(['python','client_lua_batch.py',rela_dir + _o, lua_config_path])
		print '%s\n' %  _o,
		exit_code = p.wait()
		if exit_code == 0:
			#print('\tDONE!')
			break
		print('Failed,ExitCode ' + str(exit_code))
		os.system("pause")


print("DONE")
#os.system("pause")