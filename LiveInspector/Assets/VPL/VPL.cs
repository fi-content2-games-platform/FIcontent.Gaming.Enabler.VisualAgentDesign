using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Dbg = System.Diagnostics.Debug;

namespace Aseba
{
	class VPL
	{
		// style constants
		const int blockSpacing = 40;
		const int blockWidth = 256;
		const int blockHeight = 256;
		const int deleteButtonSize = 64;
		const int columnWidth = 40;
		const int columnY = ((blockHeight + blockSpacing*2)-(4*columnWidth))/2;
		
		// textures
		// EventActionsSet
		static Texture2D Tex_EventActionSet_Left = Resources.Load<Texture2D>("EventActionsSet.Left");
		static Texture2D Tex_EventActionSet_Column = Resources.Load<Texture2D>("EventActionsSet.Column");
		static Texture2D Tex_EventActionSet_Close = Resources.Load<Texture2D>("EventActionsSet.Close");
		static Texture2D Tex_EventActionSet_Right = Resources.Load<Texture2D>("EventActionsSet.Right");
		// ArrowButtonsEventBlock
		static Texture2D Tex_ArrowButtonsEventBlock_Bg = Resources.Load<Texture2D>("ArrowButtonsEventBlock.Bg");
		static Texture2D Tex_ArrowButtonsEventBlock_Set = Resources.Load<Texture2D>("ArrowButtonsEventBlock.Set");
		
		
		// Helper functions
		
		private static uint[] GetButtonsValues(ushort data, uint count, ushort b)
		{
			uint[] values = new uint[count];
			for (uint i=0; i<count; ++i)
			{
				values[count-1-i] = (uint)(data % b);
				data /= b;
			}
			return values;
		}
		
		// Functions for VPL itself

		public static Texture2D CreateTexture(ushort[] description)
		{
			// parse description
			uint blockCount = description[1];
			uint[] blockTypes = new uint[blockCount];

			// retrieve block types
			uint curWord = 2;
			uint curPos = 0;
			for (uint i=0; i<blockCount; ++i)
			{
				blockTypes[i] = (uint)((description[curWord] >> (ushort)curPos) & 0xf);
				curPos += 4;
				if (curPos == 16)
				{
					curPos = 0;
					curWord += 1;
				}
			}
			curWord += 1;
			bool hasStateFilter = blockTypes[1] == 7;
			uint actionsCount = hasStateFilter ? blockCount-2 : blockCount-1;

			// compute sizes
			int width = 2*blockSpacing + blockWidth;
			if (hasStateFilter)
				width += blockSpacing + blockWidth;
			int columnPos = width;
			width += columnWidth + blockSpacing;
			width += (blockSpacing + blockWidth) * (int)actionsCount;
			width += deleteButtonSize + blockSpacing;
			int height = 2*blockSpacing + blockHeight;

			// print block types
			Debug.Log(String.Format ("Found {0} blocks of types {1} ", blockCount, String.Join(", ", Array.ConvertAll<uint, string>(blockTypes, Convert.ToString))));
		
			// allocate pixels for draw operations
			Color32[] targetPixels = new Color32[width*height];
			Painter painter = new Painter(targetPixels, width, height);
			
			// draw content
			
			// background
			painter.Fill(4, 0, width-8, height, new Color32(234,234,234,255));
			painter.Blit(0, 0, Tex_EventActionSet_Left);
			painter.Blit(columnPos, columnY, Tex_EventActionSet_Column);
			painter.Blit(width-blockSpacing-deleteButtonSize, height-blockSpacing-deleteButtonSize, Tex_EventActionSet_Close);
			painter.Blit( width-4, 0, Tex_EventActionSet_Right);
			
			// event
			switch (blockTypes[0])
			{
				case 1: curWord += CreateTextureButton(description, curWord, painter, blockSpacing, blockSpacing); break;
				default: break;
			}
			
			// create and return texture
			Texture2D texture = new Texture2D(width, height);
			texture.SetPixels32(targetPixels);
			texture.Apply();
			
			return texture;
		}
		
		public static uint CreateTextureButton(ushort[] description, uint posInDescription, Painter painter, int lx, int ly)
		{
			// background
			painter.Blit(lx, ly, Tex_ArrowButtonsEventBlock_Bg);
			
			// display button state: top, left, bottom, right, center
			uint[] buttons = GetButtonsValues(description[posInDescription], 5, 2);
			Debug.Log(String.Join(", ", Array.ConvertAll<uint, string>(buttons, Convert.ToString)));
			if (buttons[0] != 0)
				painter.Blit(lx, ly, new Scanlines { { 174, 101, 154 }, { 175, 100, 155 }, { 176, 100, 155 }, { 177, 100, 155 }, { 178, 101, 154 }, { 179, 101, 154 }, { 180, 102, 153 }, { 181, 102, 153 }, { 182, 103, 152 }, { 183, 103, 152 }, { 184, 104, 151 }, { 185, 105, 150 }, { 186, 105, 150 }, { 187, 106, 149 }, { 188, 106, 149 }, { 189, 107, 148 }, { 190, 108, 147 }, { 191, 108, 147 }, { 192, 109, 146 }, { 193, 109, 146 }, { 194, 110, 145 }, { 195, 110, 145 }, { 196, 111, 144 }, { 197, 112, 143 }, { 198, 112, 143 }, { 199, 113, 142 }, { 200, 113, 142 }, { 201, 114, 141 }, { 202, 115, 140 }, { 203, 115, 140 }, { 204, 116, 139 }, { 205, 116, 139 }, { 206, 117, 138 }, { 207, 117, 138 }, { 208, 118, 137 }, { 209, 119, 136 }, { 210, 119, 136 }, { 211, 120, 135 }, { 212, 120, 135 }, { 213, 121, 134 }, { 214, 121, 134 }, { 215, 122, 133 }, { 216, 123, 132 }, { 217, 123, 132 }, { 218, 124, 131 }, { 219, 124, 131 }, { 220, 125, 130 }, { 221, 126, 129 } }, Tex_ArrowButtonsEventBlock_Set);
			if (buttons[1] != 0)
				painter.Blit(lx, ly, new Scanlines { { 100, 78, 80 }, { 101, 76, 81 }, { 102, 74, 81 }, { 103, 72, 81 }, { 104, 71, 81 }, { 105, 69, 81 }, { 106, 67, 81 }, { 107, 66, 81 }, { 108, 64, 81 }, { 109, 62, 81 }, { 110, 60, 81 }, { 111, 59, 81 }, { 112, 57, 81 }, { 113, 55, 81 }, { 114, 54, 81 }, { 115, 52, 81 }, { 116, 50, 81 }, { 117, 48, 81 }, { 118, 47, 81 }, { 119, 45, 81 }, { 120, 43, 81 }, { 121, 41, 81 }, { 122, 40, 81 }, { 123, 38, 81 }, { 124, 36, 81 }, { 125, 35, 81 }, { 126, 34, 81 }, { 127, 34, 81 }, { 128, 34, 81 }, { 129, 34, 81 }, { 130, 35, 81 }, { 131, 36, 81 }, { 132, 38, 81 }, { 133, 40, 81 }, { 134, 41, 81 }, { 135, 43, 81 }, { 136, 45, 81 }, { 137, 47, 81 }, { 138, 48, 81 }, { 139, 50, 81 }, { 140, 52, 81 }, { 141, 54, 81 }, { 142, 55, 81 }, { 143, 57, 81 }, { 144, 59, 81 }, { 145, 60, 81 }, { 146, 62, 81 }, { 147, 64, 81 }, { 148, 66, 81 }, { 149, 67, 81 }, { 150, 69, 81 }, { 151, 71, 81 }, { 152, 72, 81 }, { 153, 74, 81 }, { 154, 76, 81 }, { 155, 78, 80 } }, Tex_ArrowButtonsEventBlock_Set);
			if (buttons[2] != 0)
				painter.Blit(lx, ly, new Scanlines { { 34, 126, 129 }, { 35, 125, 130 }, { 36, 124, 131 }, { 37, 124, 131 }, { 38, 123, 132 }, { 39, 123, 132 }, { 40, 122, 133 }, { 41, 121, 134 }, { 42, 121, 134 }, { 43, 120, 135 }, { 44, 120, 135 }, { 45, 119, 136 }, { 46, 119, 136 }, { 47, 118, 137 }, { 48, 117, 138 }, { 49, 117, 138 }, { 50, 116, 139 }, { 51, 116, 139 }, { 52, 115, 140 }, { 53, 115, 140 }, { 54, 114, 141 }, { 55, 113, 142 }, { 56, 113, 142 }, { 57, 112, 143 }, { 58, 112, 143 }, { 59, 111, 144 }, { 60, 110, 145 }, { 61, 110, 145 }, { 62, 109, 146 }, { 63, 109, 146 }, { 64, 108, 147 }, { 65, 108, 147 }, { 66, 107, 148 }, { 67, 106, 149 }, { 68, 106, 149 }, { 69, 105, 150 }, { 70, 105, 150 }, { 71, 104, 151 }, { 72, 103, 152 }, { 73, 103, 152 }, { 74, 102, 153 }, { 75, 102, 153 }, { 76, 101, 154 }, { 77, 101, 154 }, { 78, 100, 155 }, { 79, 100, 155 }, { 80, 100, 155 }, { 81, 101, 154 } }, Tex_ArrowButtonsEventBlock_Set);
			if (buttons[3] != 0)
				painter.Blit(lx, ly, new Scanlines { { 100, 175, 177 }, { 101, 174, 179 }, { 102, 174, 181 }, { 103, 174, 183 }, { 104, 174, 184 }, { 105, 174, 186 }, { 106, 174, 188 }, { 107, 174, 189 }, { 108, 174, 191 }, { 109, 174, 193 }, { 110, 174, 195 }, { 111, 174, 196 }, { 112, 174, 198 }, { 113, 174, 200 }, { 114, 174, 201 }, { 115, 174, 203 }, { 116, 174, 205 }, { 117, 174, 207 }, { 118, 174, 208 }, { 119, 174, 210 }, { 120, 174, 212 }, { 121, 174, 214 }, { 122, 174, 215 }, { 123, 174, 217 }, { 124, 174, 219 }, { 125, 174, 220 }, { 126, 174, 221 }, { 127, 174, 221 }, { 128, 174, 221 }, { 129, 174, 221 }, { 130, 174, 220 }, { 131, 174, 219 }, { 132, 174, 217 }, { 133, 174, 215 }, { 134, 174, 214 }, { 135, 174, 212 }, { 136, 174, 210 }, { 137, 174, 208 }, { 138, 174, 207 }, { 139, 174, 205 }, { 140, 174, 203 }, { 141, 174, 201 }, { 142, 174, 200 }, { 143, 174, 198 }, { 144, 174, 196 }, { 145, 174, 195 }, { 146, 174, 193 }, { 147, 174, 191 }, { 148, 174, 189 }, { 149, 174, 188 }, { 150, 174, 186 }, { 151, 174, 184 }, { 152, 174, 183 }, { 153, 174, 181 }, { 154, 174, 179 }, { 155, 175, 177 } }, Tex_ArrowButtonsEventBlock_Set);
			if (buttons[4] != 0)
				painter.Blit(lx, ly, new Scanlines { { 100, 123, 132 }, { 101, 119, 136 }, { 102, 116, 139 }, { 103, 114, 141 }, { 104, 113, 142 }, { 105, 111, 144 }, { 106, 110, 145 }, { 107, 109, 146 }, { 108, 108, 147 }, { 109, 107, 148 }, { 110, 106, 149 }, { 111, 105, 150 }, { 112, 105, 150 }, { 113, 104, 151 }, { 114, 103, 152 }, { 115, 103, 152 }, { 116, 102, 153 }, { 117, 102, 153 }, { 118, 102, 153 }, { 119, 101, 154 }, { 120, 101, 154 }, { 121, 101, 154 }, { 122, 101, 154 }, { 123, 100, 155 }, { 124, 100, 155 }, { 125, 100, 155 }, { 126, 100, 155 }, { 127, 100, 155 }, { 128, 100, 155 }, { 129, 100, 155 }, { 130, 100, 155 }, { 131, 100, 155 }, { 132, 100, 155 }, { 133, 101, 154 }, { 134, 101, 154 }, { 135, 101, 154 }, { 136, 101, 154 }, { 137, 102, 153 }, { 138, 102, 153 }, { 139, 102, 153 }, { 140, 103, 152 }, { 141, 103, 152 }, { 142, 104, 151 }, { 143, 105, 150 }, { 144, 105, 150 }, { 145, 106, 149 }, { 146, 107, 148 }, { 147, 108, 147 }, { 148, 109, 146 }, { 149, 110, 145 }, { 150, 111, 144 }, { 151, 113, 142 }, { 152, 114, 141 }, { 153, 116, 139 }, { 154, 119, 136 }, { 155, 123, 132 } }, Tex_ArrowButtonsEventBlock_Set);
			
			// to copy-paste for later
			//if (buttons[0] != 0)
			//	painter.Blit(lx, ly, , Tex_ArrowButtonsEventBlock_Set);
			
			return 1;
		}
	};
}
