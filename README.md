# Sokoban(推箱子)
本程序是用 **<font color=red>C#**</font> 语言和 **<font color=red>WPF</font>** 技术共同开发。其界面简洁，操作简单，支持鼠标点击素材构建地图。由于本人算法水平有限，尚未对构建的地图进行验证是否可行，后续开发将会提供撤销和重做功能，详情请点击程序相应的界面。  

本程序使用的关卡地图采用通用的 **<font color=red>XSB</font>** 格式，如下：  
字符&nbsp;&nbsp;&nbsp;含义  
@ ==>人 (man)  
\+ ==>人在目标点 (man on goal)  
$ ==>箱子 (box)  
\* ==>箱子在目标点 (box on goal)  
\# ==>墙 (wall)  
. ==>目标点 (goal)  
\- ==>地板 (floor)，本程序规定墙体外面空白区域用“_”代替。  

例子 (example)  
____#####\_\_\_\_\_\_\_\_\_\_  
____#\-\-\-#\_\_\_\_\_\_\_\_\_\_  
____#\$\-\-#\_\_\_\_\_\_\_\_\_\_  
__###\-\-\$##\_\_\_\_\_\_\_\_\_  
__#\-\-\$-$-#\_\_\_\_\_\_\_\_\_  
###\-#\-##\-#\-\-\-######  
#\-\-\-#-##\-#####\-\-\.\.#  
#-\$\-\-\$----------\.\.#  
#####-###-#@##\-\-\.\.#  
____#-----#########  
____#######\_\_\_\_\_\_\_\_  

**声明：本程序所用到的游戏素材（例如：图片、音频）来源均来自互联网。<font color=red>仅用于编程学习交流，却勿用于商业用途。</font>**  
**Note:&nbsp;&nbsp;All game materials (such as pictures, audio) used in this program are from the Internet. <font color=red>It is only used for programming learning and communication, but not for business practice.</font>**