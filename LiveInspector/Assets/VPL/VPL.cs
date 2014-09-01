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
		// ProxEventBlock
		static Texture2D Tex_ProxEventBlock_Bg = Resources.Load<Texture2D>("ProxEventBlock.Bg");
		static Texture2D Tex_ProxEventBlock_Near = Resources.Load<Texture2D>("ProxEventBlock.Near");
		static Texture2D Tex_ProxEventBlock_Far = Resources.Load<Texture2D>("ProxEventBlock.Far");
		// ProxGroundEventBlock
		static Texture2D Tex_ProxGroundEventBlock_Bg = Resources.Load<Texture2D>("ProxGroundEventBlock.Bg");
		static Texture2D Tex_ProxGroundEventBlock_Near = Resources.Load<Texture2D>("ProxGroundEventBlock.Near");
		static Texture2D Tex_ProxGroundEventBlock_Far = Resources.Load<Texture2D>("ProxGroundEventBlock.Far");
		// other events
		static Texture2D Tex_TapEventBlock_Simple = Resources.Load<Texture2D>("TapEventBlock.Simple");
		static Texture2D Tex_ClapEventBlock = Resources.Load<Texture2D>("ClapEventBlock");
		static Texture2D Tex_TimeoutEventBlock = Resources.Load<Texture2D>("TimeoutEventBlock");
		
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
				case 2: curWord += CreateTextureProx(description, curWord, painter, blockSpacing, blockSpacing); break;
				case 3: curWord += CreateTextureProxGround(description, curWord, painter, blockSpacing, blockSpacing); break;
				case 4: painter.Blit(blockSpacing, blockSpacing, Tex_TapEventBlock_Simple); curWord += 1; break;
				case 5: painter.Blit(blockSpacing, blockSpacing, Tex_ClapEventBlock); break;
				case 6: painter.Blit(blockSpacing, blockSpacing, Tex_TimeoutEventBlock); break;
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
			
			return 1;
		}
		
		public static uint CreateTextureProx(ushort[] description, uint posInDescription, Painter painter, int lx, int ly)
		{
			// background
			painter.Blit(lx, ly, Tex_ProxEventBlock_Bg);
			
			// display sensor state: front left to right, then back left to right
			uint[] buttons = GetButtonsValues(description[posInDescription], 7, 3);
			Scanlines[] masks = {
				new Scanlines { { 167, 11, 17 }, { 168, 9, 20 }, { 169, 8, 21 }, { 170, 7, 29 }, { 171, 6, 31 }, { 172, 5, 32 }, { 173, 5, 34 }, { 174, 5, 35 }, { 175, 5, 36 }, { 176, 5, 37 }, { 177, 6, 38 }, { 178, 6, 40 }, { 179, 6, 41 }, { 180, 6, 42 }, { 181, 6, 43 }, { 182, 7, 44 }, { 183, 7, 46 }, { 184, 7, 47 }, { 185, 7, 48 }, { 186, 7, 49 }, { 187, 8, 50 }, { 188, 8, 51 }, { 189, 10, 53 }, { 190, 11, 54 }, { 191, 10, 55 }, { 192, 9, 56 }, { 193, 9, 56 }, { 194, 8, 56 }, { 195, 7, 56 }, { 196, 6, 55 }, { 197, 6, 54 }, { 198, 6, 53 }, { 199, 7, 52 }, { 200, 7, 51 }, { 201, 9, 51 }, { 202, 10, 50 }, { 203, 11, 49 }, { 204, 12, 48 }, { 205, 13, 47 }, { 206, 14, 47 }, { 207, 16, 46 }, { 208, 17, 45 }, { 209, 18, 44 }, { 210, 19, 43 }, { 211, 20, 42 }, { 212, 22, 41 }, { 213, 23, 40 }, { 214, 24, 40 }, { 215, 25, 39 }, { 216, 26, 38 }, { 217, 28, 37 }, { 218, 29, 36 }, { 219, 30, 35 }, { 220, 31, 34 } },
				new Scanlines { { 199, 65, 70 }, { 200, 64, 73 }, { 201, 64, 75 }, { 202, 52, 78 }, { 203, 51, 81 }, { 204, 50, 84 }, { 205, 49, 86 }, { 206, 48, 89 }, { 207, 47, 92 }, { 208, 46, 95 }, { 209, 45, 97 }, { 210, 44, 98 }, { 211, 44, 99 }, { 212, 43, 99 }, { 213, 43, 99 }, { 214, 43, 99 }, { 215, 42, 98 }, { 216, 42, 98 }, { 217, 43, 98 }, { 218, 43, 97 }, { 219, 44, 97 }, { 220, 44, 96 }, { 221, 45, 96 }, { 222, 46, 95 }, { 223, 48, 95 }, { 224, 49, 95 }, { 225, 52, 94 }, { 226, 55, 94 }, { 227, 54, 94 }, { 228, 54, 93 }, { 229, 54, 93 }, { 230, 53, 93 }, { 231, 53, 92 }, { 232, 53, 92 }, { 233, 54, 91 }, { 234, 55, 91 }, { 235, 58, 91 }, { 236, 61, 90 }, { 237, 64, 90 }, { 238, 66, 90 }, { 239, 69, 89 }, { 240, 72, 89 }, { 241, 75, 89 }, { 242, 77, 88 }, { 243, 80, 88 }, { 244, 83, 87 } },
				new Scanlines { { 212, 110, 144 }, { 213, 110, 146 }, { 214, 109, 147 }, { 215, 108, 147 }, { 216, 108, 147 }, { 217, 108, 147 }, { 218, 108, 147 }, { 219, 100, 155 }, { 220, 98, 157 }, { 221, 97, 158 }, { 222, 97, 158 }, { 223, 97, 158 }, { 224, 96, 159 }, { 225, 96, 159 }, { 226, 95, 160 }, { 227, 95, 160 }, { 228, 95, 160 }, { 229, 94, 161 }, { 230, 94, 161 }, { 231, 94, 162 }, { 232, 93, 162 }, { 233, 93, 162 }, { 234, 93, 162 }, { 235, 93, 162 }, { 236, 93, 161 }, { 237, 94, 161 }, { 238, 95, 160 }, { 239, 96, 159 }, { 240, 98, 158 }, { 241, 100, 156 }, { 242, 108, 147 }, { 243, 108, 147 }, { 244, 108, 147 }, { 245, 108, 147 }, { 246, 108, 147 }, { 247, 109, 146 }, { 248, 109, 145 }, { 249, 111, 145 } },
				new Scanlines { { 199, 185, 190 }, { 200, 182, 191 }, { 201, 180, 191 }, { 202, 177, 203 }, { 203, 174, 204 }, { 204, 171, 205 }, { 205, 169, 206 }, { 206, 166, 207 }, { 207, 163, 208 }, { 208, 160, 209 }, { 209, 158, 210 }, { 210, 157, 211 }, { 211, 156, 211 }, { 212, 156, 212 }, { 213, 156, 212 }, { 214, 156, 212 }, { 215, 157, 213 }, { 216, 157, 213 }, { 217, 157, 212 }, { 218, 158, 212 }, { 219, 158, 211 }, { 220, 159, 211 }, { 221, 159, 210 }, { 222, 160, 209 }, { 223, 160, 207 }, { 224, 160, 206 }, { 225, 161, 203 }, { 226, 161, 200 }, { 227, 161, 201 }, { 228, 162, 201 }, { 229, 162, 201 }, { 230, 162, 202 }, { 231, 163, 202 }, { 232, 163, 202 }, { 233, 164, 201 }, { 234, 164, 200 }, { 235, 164, 197 }, { 236, 165, 194 }, { 237, 165, 191 }, { 238, 165, 189 }, { 239, 166, 186 }, { 240, 166, 183 }, { 241, 166, 180 }, { 242, 167, 178 }, { 243, 167, 175 }, { 244, 168, 172 } },
				new Scanlines { { 167, 238, 244 }, { 168, 235, 246 }, { 169, 234, 247 }, { 170, 226, 248 }, { 171, 224, 249 }, { 172, 223, 250 }, { 173, 221, 250 }, { 174, 220, 250 }, { 175, 219, 250 }, { 176, 218, 250 }, { 177, 217, 249 }, { 178, 215, 249 }, { 179, 214, 249 }, { 180, 213, 249 }, { 181, 212, 249 }, { 182, 211, 248 }, { 183, 209, 248 }, { 184, 208, 248 }, { 185, 207, 248 }, { 186, 206, 248 }, { 187, 205, 247 }, { 188, 204, 247 }, { 189, 202, 245 }, { 190, 201, 244 }, { 191, 200, 245 }, { 192, 199, 246 }, { 193, 199, 246 }, { 194, 199, 247 }, { 195, 199, 248 }, { 196, 200, 249 }, { 197, 201, 249 }, { 198, 202, 249 }, { 199, 203, 248 }, { 200, 204, 248 }, { 201, 204, 246 }, { 202, 205, 245 }, { 203, 206, 244 }, { 204, 207, 243 }, { 205, 208, 242 }, { 206, 208, 241 }, { 207, 209, 239 }, { 208, 210, 238 }, { 209, 211, 237 }, { 210, 212, 236 }, { 211, 213, 235 }, { 212, 214, 233 }, { 213, 215, 232 }, { 214, 215, 231 }, { 215, 216, 230 }, { 216, 217, 229 }, { 217, 218, 227 }, { 218, 219, 226 }, { 219, 220, 225 }, { 220, 221, 224 } },
				new Scanlines { { 3, 47, 80 }, { 4, 46, 81 }, { 5, 45, 82 }, { 6, 45, 82 }, { 7, 45, 82 }, { 8, 45, 82 }, { 9, 45, 82 }, { 10, 45, 82 }, { 11, 45, 82 }, { 12, 35, 82 }, { 13, 33, 82 }, { 14, 32, 82 }, { 15, 31, 82 }, { 16, 31, 82 }, { 17, 30, 82 }, { 18, 30, 82 }, { 19, 29, 82 }, { 20, 29, 82 }, { 21, 29, 82 }, { 22, 29, 82 }, { 23, 29, 82 }, { 24, 29, 82 }, { 25, 29, 82 }, { 26, 30, 82 }, { 27, 30, 82 }, { 28, 31, 82 }, { 29, 32, 82 }, { 30, 32, 82 }, { 31, 34, 82 }, { 32, 36, 82 }, { 33, 45, 82 }, { 34, 45, 82 }, { 35, 45, 82 }, { 36, 45, 82 }, { 37, 45, 82 }, { 38, 45, 82 }, { 39, 46, 81 }, { 40, 47, 80 } },
				new Scanlines { { 3, 175, 208 }, { 4, 174, 209 }, { 5, 173, 210 }, { 6, 173, 210 }, { 7, 173, 210 }, { 8, 173, 210 }, { 9, 173, 210 }, { 10, 173, 210 }, { 11, 173, 210 }, { 12, 173, 220 }, { 13, 173, 222 }, { 14, 173, 223 }, { 15, 173, 224 }, { 16, 173, 224 }, { 17, 173, 225 }, { 18, 173, 225 }, { 19, 173, 226 }, { 20, 173, 226 }, { 21, 173, 226 }, { 22, 173, 226 }, { 23, 173, 226 }, { 24, 173, 226 }, { 25, 173, 226 }, { 26, 173, 225 }, { 27, 173, 225 }, { 28, 173, 224 }, { 29, 173, 223 }, { 30, 173, 223 }, { 31, 173, 221 }, { 32, 173, 219 }, { 33, 173, 210 }, { 34, 173, 210 }, { 35, 173, 210 }, { 36, 173, 210 }, { 37, 173, 210 }, { 38, 173, 210 }, { 39, 174, 209 }, { 40, 175, 208 } }
			};
			for (uint i=0; i<7; ++i)
			{
				if (buttons[i] == 1)
					painter.Blit(lx, ly, masks[i], Tex_ProxEventBlock_Near);
				else if (buttons[i] == 2)
					painter.Blit(lx, ly, masks[i], Tex_ProxEventBlock_Far);
			}
			
			return 1;
		}
		
		public static uint CreateTextureProxGround(ushort[] description, uint posInDescription, Painter painter, int lx, int ly)
		{
			// background
			painter.Blit(lx, ly, Tex_ProxGroundEventBlock_Bg);
			
			// display sensor state: left then right
			uint[] buttons = GetButtonsValues(description[posInDescription], 2, 3);
			Scanlines[] masks = {
				new Scanlines { { 197, 80, 115 }, { 198, 79, 116 }, { 199, 78, 116 }, { 200, 78, 116 }, { 201, 78, 116 }, { 202, 78, 116 }, { 203, 78, 116 }, { 204, 78, 116 }, { 205, 67, 116 }, { 206, 65, 116 }, { 207, 64, 116 }, { 208, 63, 116 }, { 209, 63, 116 }, { 210, 62, 116 }, { 211, 62, 116 }, { 212, 61, 116 }, { 213, 61, 116 }, { 214, 61, 116 }, { 215, 61, 116 }, { 216, 61, 116 }, { 217, 61, 116 }, { 218, 61, 116 }, { 219, 61, 116 }, { 220, 62, 116 }, { 221, 62, 116 }, { 222, 63, 116 }, { 223, 64, 116 }, { 224, 65, 116 }, { 225, 66, 116 }, { 226, 69, 116 }, { 227, 78, 116 }, { 228, 78, 116 }, { 229, 78, 116 }, { 230, 78, 116 }, { 231, 78, 116 }, { 232, 79, 116 }, { 233, 79, 116 }, { 234, 80, 114 } },
				new Scanlines { { 197, 140, 175 }, { 198, 139, 176 }, { 199, 138, 176 }, { 200, 138, 176 }, { 201, 138, 176 }, { 202, 138, 176 }, { 203, 138, 176 }, { 204, 138, 176 }, { 205, 138, 188 }, { 206, 138, 189 }, { 207, 138, 190 }, { 208, 138, 191 }, { 209, 138, 192 }, { 210, 138, 193 }, { 211, 138, 193 }, { 212, 138, 194 }, { 213, 138, 194 }, { 214, 138, 194 }, { 215, 138, 194 }, { 216, 138, 194 }, { 217, 138, 194 }, { 218, 138, 194 }, { 219, 138, 193 }, { 220, 138, 193 }, { 221, 138, 193 }, { 222, 138, 192 }, { 223, 138, 191 }, { 224, 138, 190 }, { 225, 138, 188 }, { 226, 138, 186 }, { 227, 138, 176 }, { 228, 138, 176 }, { 229, 138, 176 }, { 230, 138, 176 }, { 231, 138, 176 }, { 232, 139, 176 }, { 233, 139, 176 }, { 234, 140, 174 } }
			};
			for (uint i=0; i<2; ++i)
			{
				if (buttons[i] == 1)
					painter.Blit(lx, ly, masks[i], Tex_ProxGroundEventBlock_Near);
				else if (buttons[i] == 2)
					painter.Blit(lx, ly, masks[i], Tex_ProxGroundEventBlock_Far);
			}
			
			return 1;
		}
		
		// to copy-paste for later
			//if (buttons[0] != 0)
			//	
	};
}
