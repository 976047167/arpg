from PIL import Image
import numpy as np
import sys
import math
class Point:
	def __init__(self,x=0,y=0):
		self.x = x
		self.y = y
		self.sqrLength = self.x *self.x +self.y *self.y
	def newClone( self ,dx =0 ,dy = 0):
		return Point(self.x+dx,self.y+dy)
	@staticmethod
	def Inside():
		return Point()
	@staticmethod
	def Empyt():
		return Point(9999,9999)
class Grid:
	def __init__(self,array,reverse = False):
		self.arr = np.empty(array.shape,dtype=Point)
		self.height = self.arr.shape[0]
		self.width = self.arr.shape[1]
		x = 0
		while(x<self.width):
			y = 0
			while(y<self.height):
				pixel = array[y][x]
				if((pixel<128 )^ reverse ):
					self.put(x,y,Point.Inside())
				else:
					self.put(x,y,Point.Empyt())
				y+=1
			x+=1
	def get(self,x,y):
		if(x<0 or y<0 or x>=self.width or y>=self.height):
			return Point.Empyt()
		else:
			return self.arr[y,x]
	def put(self,x,y,p):
		self.arr[y][x] = p
	def compare(self,x,y,dx,dy):
		point =  self.get(x,y)
		other = self.get(x+dx,y+dy)
		near = other.newClone(dx,dy)
		# if(near.sqrLength<0):
		# 	print(x,y)
		if(point.sqrLength >= near.sqrLength):
			self.put(x,y,near)
	def GenerateSDF(self):
		y = 0
		while(y<self.height):
			x =0
			while (x<self.width):
				self.compare(x,y,-1,0)
				self.compare(x,y,0,-1)
				self.compare(x,y,-1,-1)
				self.compare(x,y,1,-1)
				x+=1
			x = self.width -1
			while(x>=0):
				self.compare(x,y,1,0)
				x-=1
			y+=1

		y = self.height -1
		while(y>=0):
			x = self.width -1
			while (x>=0):
				self.compare(x,y,1,0)
				self.compare(x,y,0,1)
				self.compare(x,y,-1,1)
				self.compare(x,y,1,1)
				x -= 1
			x = 0
			while(x<self.width):
				self.compare(x,y,-1,0)
				x += 1
			y-=1


im = Image.open("test.bmp")
R= im.split()[0]
arr = np.array(R)
grid1 = Grid(arr)
grid2 = Grid(arr,True)
grid1.GenerateSDF()
grid2.GenerateSDF()
ret = np.empty(arr.shape)
y = arr.shape[0]-1
while(y>=0):
	x = arr.shape[1]-1
	while(x>=0):
		# print(x,y)
		# print(grid1.get(x,y).sqrLength)
		dist1 = math.sqrt(grid1.get(x,y).sqrLength)
		dist2 = math.sqrt(grid2.get(x,y).sqrLength)
		dist = (dist1 - dist2)*3+128
		ret[y][x] = dist
		x-=1
	y-=1
new_im = Image.fromarray(ret).convert('L')
new_im.show()
print(new_im.getbands(), new_im.size, new_im.mode)
	