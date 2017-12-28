import os
import sys
import string

client_config_dir = "../pbconfig"
loadfile_name = "ConfigLoader.cs"
config_namespace = "SanguoConfig"

allfile = os.listdir(client_config_dir)
cs_names = []
for _file in allfile :
	if not _file.startswith("cs_"):
		continue
	if not _file.endswith(".cs"):
		continue
	cs_names.append(_file[0:_file.rfind('.')])

file_handle = open(client_config_dir +"/" +loadfile_name, 'w+');
file_handle.write("using System;\nusing System.Collections.Generic;\nusing "
"System.Text;\nusing System.IO;\nusing ProtoBuf;")
file_handle.write("\n\n\n")

file_handle.write("namespace SanguoConfig\n{\n")
file_handle.write("\tpublic class SanguoConfigLoader\n\t{\n")
for _o in cs_names:
	objName = _o.replace("cs_","") + "Cfg"
	fullname = config_namespace + "." + _o
	file_handle.write("\t\tpublic static "+fullname + " "+objName  +" = new " + fullname + "();\n")
	
file_handle.write("\n\t\tpublic static void Load()\n")
file_handle.write("\t\t{\n")
for _o in cs_names:

	objName = _o.replace("cs_","") + "Cfg"
	fullname = config_namespace + "." + _o
	
	file_handle.write("\t\t\tusing (var file = File.OpenRead(\"" + _o + ".dat\"))\n")
	file_handle.write("\t\t\t{\n")
	file_handle.write("\t\t\t\t" + objName + " = Serializer.Deserialize<" + fullname + ">(file);\n"  )
	file_handle.write("\t\t\t}\n")
	file_handle.write("\n")
	
file_handle.write("\t\t}\n")
file_handle.write("\t}\n")
file_handle.write("}\n")