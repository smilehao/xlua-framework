
import sys
from google.protobuf import descriptor

'''
role_cfg = cs_role_pb2.cs_role()

fs = open('cs_role.dat','rb')
all_str = fs.read()
role_cfg.ParseFromString(all_str)
target = sys.stdout
'''
def PrintMessage(message, out, col_hook,indent=0, as_utf8=False, as_one_line=False):
  for field, value in message.ListFields():
    if field.label == descriptor.FieldDescriptor.LABEL_REPEATED:
      for element in value:
		id =  getattr(element,'id')
		id_str = str(id)
		out.write('[' + id_str + '] = ')
		PrintField(field, element, out, col_hook, indent, as_utf8, as_one_line)
    else:
      PrintField(field, value, out, col_hook, indent, as_utf8, as_one_line)

def PrintField(field, value, out, col_hook,indent=0, as_utf8=False, as_one_line=False):
  """Print a single field name/value pair.  For repeated fields, the value
  should be a single element."""

  out.write(' ' * indent);

  if field.cpp_type == descriptor.FieldDescriptor.CPPTYPE_MESSAGE:
    out.write('')
  else:
    out.write(field.name  + ' = ' )

  PrintFieldValue(field, value, out,col_hook, indent, as_utf8, as_one_line)
  if as_one_line:
    out.write(' ')
  else:
    out.write(',\n')


def PrintFieldValue(field, value, out, col_hook,indent=0,
                    as_utf8=False, as_one_line=False):
  """Print a single field value (not including name).  For repeated fields,
  the value should be a single element."""

  if field.cpp_type == descriptor.FieldDescriptor.CPPTYPE_MESSAGE:
    if as_one_line:
      out.write(' { ')
      PrintMessage(value, out, col_hook,indent, as_utf8, as_one_line)
      out.write('}')
    else:
      out.write(' {\n')
      PrintMessage(value, out, col_hook,indent + 2, as_utf8, as_one_line)
      out.write(' ' * indent + '}')
  elif field.cpp_type == descriptor.FieldDescriptor.CPPTYPE_ENUM:
    enum_value = field.enum_type.values_by_number.get(value, None)
    if enum_value is not None:
      out.write(enum_value.name)
    else:
      out.write(str(value))
  elif field.cpp_type == descriptor.FieldDescriptor.CPPTYPE_STRING:
	if not col_hook or not col_hook(field.name,value,out):
		out.write('\"')
		
		if type(value) is unicode:
		  out.write(value.encode('utf-8'))
		else:
		  out.write(value)
		out.write('\"')

		
  elif field.cpp_type == descriptor.FieldDescriptor.CPPTYPE_BOOL:
    if value:
      out.write("true")
    else:
      out.write("false")
  else:
    out.write(str(value))

def toluaaaa(root_cfg,target_file,col_hook):
	target_file.write('local ' + root_cfg.__class__.__name__ + " = {\n")
	PrintMessage(root_cfg,target_file,col_hook)
	target_file.write("}\n")
	target_file.write("return " + root_cfg.__class__.__name__ );
