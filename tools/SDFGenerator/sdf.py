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
	def __init__(self,array):
		self.arr = np.empty(array.shape,dtype=Point)
		self.rev_arr = np.empty(array.shape,dtype=Point)
		self.height = self.arr.shape[0]
		self.width = self.arr.shape[1]
		x = 0
		while(x<self.width):
			y = 0
			while(y<self.height):
				pixel = array[y][x]
				if((pixel<128 )):
					self.put(x,y,Point.Inside())
					self.put(x,y,Point.Empyt(),True)
				else:
					self.put(x,y,Point.Empyt())
					self.put(x,y,Point.Inside(),True)
				y+=1
			x+=1
	def get(self,x,y,reverse = False):
		if(x<0 or y<0 or x>=self.width or y>=self.height):
			return Point.Empyt()
		else:
			if (reverse):
				return self.rev_arr[y][x]
			return self.arr[y][x]
	def put(self,x,y,p,reverse = False):
		if(reverse):
			self.rev_arr[y][x]=p
		else:
			self.arr[y][x] = p
	def compare(self,x,y,dx,dy):
		point =  self.get(x,y)
		other = self.get(x+dx,y+dy)
		near = other.newClone(dx,dy)
		if(point.sqrLength >= near.sqrLength):
			self.put(x,y,near)
		point =  self.get(x,y,True)
		other = self.get(x+dx,y+dy,True)
		near = other.newClone(dx,dy)
		if(point.sqrLength >= near.sqrLength):
			self.put(x,y,near,True)
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
		ret = np.empty(self.arr.shape)
		y = self.height -1
		while(y>=0):
			x = self.width -1
			while(x>=0):
				# print(x,y)
				dist1 = math.sqrt(self.get(x,y).sqrLength)
				dist2 = math.sqrt(self.get(x,y,True).sqrLength)
				dist = (dist1 - dist2)*3+128
				ret[y][x] = dist
				x-=1
			y-=1
		im_ret = Image.fromarray(ret).convert('L')
		return im_ret


im = Image.open("test.bmp")
R= im.split()[0]
arr = np.array(R)
grid = Grid(arr)
new_im  = grid.GenerateSDF()
new_im.show()
print(new_im.getbands(), new_im.size, new_im.mode)
	