import os
import sys
xlsx_name = sys.argv[1]
real_len = xlsx_name.rfind('/')
xlsx_name = xlsx_name[real_len + 1:]
xlsx_name = xlsx_name[0:xlsx_name.rfind('.') ]
print xlsx_name
#filehandler = open(xlsx_name)