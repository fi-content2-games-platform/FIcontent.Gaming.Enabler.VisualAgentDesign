using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Dbg = System.Diagnostics.Debug;

namespace Aseba
{
	public class Scanline
	{
		public int y { get; private set; }
		public int x_start { get; private set; }
		public int x_end { get; private set; }
		
		public Scanline(int y, int x_start, int x_end)
		{
			this.y = y;
			this.x_start = x_start;
			this.x_end = x_end;
		}
	};
	
	public class Scanlines : List<Scanline>
	{
		public void Add(int y, int x_start, int x_end)
		{
			Add( new Scanline(y, x_start, x_end) );
		}
	};

	class Painter
	{
		private Color32[] targetPixels;
		private int tw;
		private int th;
		
		// Constructor
		
		public Painter(Color32[] targetPixels, int tw, int th)
		{
			this.targetPixels = targetPixels;
			this.tw = tw;
			this.th = th;
		}
		
		// Blit
		
		public void Blit(int x, int y, Texture2D source)
		{
			Dbg.Assert (0 < x, "Access out of bounds on x: underflow");
			Dbg.Assert (x < tw-source.width, "Access out of bounds on x: overflow");
			Dbg.Assert (0 < y, "Access out of bounds on y: underflow");
			Dbg.Assert (y < th-source.height, "Access out of bounds on y: overflow");
			Color32[] pixels = source.GetPixels32();
			for (int dy = 0; dy < source.height; ++dy)
				for (int dx = 0; dx < source.width; ++dx)
					targetPixels[(y+dy)*tw + (x+dx)] = pixels[dy*source.width + dx]; 
		}
		
		public void Blit(Texture2D source)
		{
			Blit(0, 0, source);
		}
		
		public void Blit(int x, int y, Scanlines lines, Texture2D source)
		{
			// Note: no boundary check here because in our python script we trust!
			Color32[] pixels = source.GetPixels32();
			foreach (Scanline l in lines)
				for (int dx = l.x_start; dx < l.x_end; ++dx)
					targetPixels[(y+l.y)*tw + (x+dx)] = pixels[l.y*source.width + dx]; 
		}
		
		public void Blit(Scanlines lines, Texture2D source)
		{
			Blit(0, 0, lines, source);
		}
		
		// Fill
		
		public void Fill(int x, int y, int w, int h, Color32 color)
		{
			Dbg.Assert (0 < x, "Access out of bounds on x: underflow");
			Dbg.Assert (x < tw-w, "Access out of bounds on x: overflow");
			Dbg.Assert (0 < y, "Access out of bounds on y: underflow");
			Dbg.Assert (y < th-h, "Access out of bounds on y: overflow");
			for (int dy = 0; dy < h; ++dy)
				for (int dx = 0; dx < w; ++dx)
					targetPixels[(y+dy)*tw + (x+dx)] = color;
		}
		
		public void Fill(Color32 color)
		{
			Fill(0, 0, tw, th, color);
		}
	};
}
