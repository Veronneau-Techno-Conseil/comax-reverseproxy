import os
from jinja2 import Environment, PackageLoader, select_autoescape

print(os.getcwd())

def getText(path):
    f = open(path, "r")
    retVal = f.read().strip(' \n\r')
    print(retVal)
    f.close()
    return retVal

def processTemplate(vnum: str, template: str, tgt_folder: str):
    env = Environment(
        loader=PackageLoader("model"),
        autoescape=select_autoescape()
    )
    chartVersion = getText(f"{tgt_folder}/VERSION")
    tpl = env.get_template(template)
    tgt = open(f"{tgt_folder}/Chart.yaml", "w")
    tgt.write(tpl.render(version=vnum, chartVersion=chartVersion))
    tgt.close()


version = getText("./VERSION")

processTemplate(version, "Chart.yaml.jinja", "./helm")


