using System.Collections.Generic;
using Godot;

namespace MineImatorSimplyRemade;

/*
 	Based on the Public Domain MaxRectsBinPack.cpp source by Jukka Jylänki
 	https://github.com/juj/RectangleBinPack/
 
 	Ported to C# by Sven Magnus
 	https://github.com/jderrough/UnitySlippyMap/blob/master/Assets/UnitySlippyMap/Helpers/MaxRectsBinPack.cs
 	
    Modified for Godot by Naz Ikhsan
 	This version is also public domain - do whatever you want with it.
*/

public class MaxRectsBinPack
{

    public int binWidth = 0;
    public int binHeight = 0;
    public bool allowRotations;

    public List<Rect2I> usedRectangles = new List<Rect2I>();
    public List<Rect2I> freeRectangles = new List<Rect2I>();

    public enum FreeRectChoiceHeuristic
    {
        RectBestShortSideFit, //< -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
        RectBestLongSideFit, //< -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
        RectBestAreaFit, //< -BAF: Positions the rectangle into the smallest free rect into which it fits.
        RectBottomLeftRule, //< -BL: Does the Tetris placement.
        RectContactPointRule //< -CP: Choosest the placement where the rectangle touches other rects as much as possible.
    };

    public MaxRectsBinPack(int width, int height, bool rotations = true)
    {
        Init(width, height, rotations);
    }

    public void Init(int width, int height, bool rotations = true)
    {
        binWidth = width;
        binHeight = height;
        allowRotations = rotations;

        Rect2I n = new Rect2I(0, 0, width, height);

        usedRectangles.Clear();

        freeRectangles.Clear();
        freeRectangles.Add(n);
    }

    public Rect2I Insert(int width, int height, FreeRectChoiceHeuristic method)
    {
        Rect2I newNode = new Rect2I();
        int score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
        int score2 = 0;
        switch (method)
        {
            case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
            case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
            case FreeRectChoiceHeuristic.RectContactPointRule: newNode = FindPositionForNewNodeContactPoint(width, height, ref score1); break;
            case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
            case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
        }

        if (newNode.Size.Y == 0)
            return newNode;

        int numRectanglesToProcess = freeRectangles.Count;
        for (int i = 0; i < numRectanglesToProcess; ++i)
        {
            if (SplitFreeNode(freeRectangles[i], ref newNode))
            {
                freeRectangles.RemoveAt(i);
                --i;
                --numRectanglesToProcess;
            }
        }

        PruneFreeList();

        usedRectangles.Add(newNode);
        return newNode;
    }

    public void Insert(List<Rect2I> rects, List<Rect2I> dst, FreeRectChoiceHeuristic method)
    {
        dst.Clear();

        while (rects.Count > 0)
        {
            int bestScore1 = int.MaxValue;
            int bestScore2 = int.MaxValue;
            int bestRectIndex = -1;
            Rect2I bestNode = new Rect2I();

            for (int i = 0; i < rects.Count; ++i)
            {
                int score1 = 0;
                int score2 = 0;
                Rect2I newNode = ScoreRect2I((int)rects[i].Size.X, (int)rects[i].Size.Y, method, ref score1, ref score2);

                if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
                {
                    bestScore1 = score1;
                    bestScore2 = score2;
                    bestNode = newNode;
                    bestRectIndex = i;
                }
            }

            if (bestRectIndex == -1)
                return;

            PlaceRect2I(bestNode);
            rects.RemoveAt(bestRectIndex);
        }
    }

    public void Remove(Rect2I rect)
    {
        usedRectangles.Remove(rect);
        freeRectangles.Add(rect);
        PruneFreeList();
    }

    void PlaceRect2I(Rect2I node)
    {
        int numRectanglesToProcess = freeRectangles.Count;
        for (int i = 0; i < numRectanglesToProcess; ++i)
        {
            if (SplitFreeNode(freeRectangles[i], ref node))
            {
                freeRectangles.RemoveAt(i);
                --i;
                --numRectanglesToProcess;
            }
        }

        PruneFreeList();

        usedRectangles.Add(node);
    }

    Rect2I ScoreRect2I(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2)
    {
        Rect2I newNode = new Rect2I();
        score1 = int.MaxValue;
        score2 = int.MaxValue;
        switch (method)
        {
            case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
            case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
            case FreeRectChoiceHeuristic.RectContactPointRule: newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
                score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
                break;
            case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
            case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
        }

        // Cannot fit the current rectangle.
        if (newNode.Size.Y == 0)
        {
            score1 = int.MaxValue;
            score2 = int.MaxValue;
        }

        return newNode;
    }

    /// Computes the ratio of used surface area.
    public float Occupancy()
    {
        ulong usedSurfaceArea = 0;
        for (int i = 0; i < usedRectangles.Count; ++i)
            usedSurfaceArea += (uint)usedRectangles[i].Size.X * (uint)usedRectangles[i].Size.Y;

        return (float)usedSurfaceArea / (binWidth * binHeight);
    }

    Rect2I FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
    {
        Rect2I bestNode = new Rect2I();
        //memset(bestNode, 0, sizeof(Rect));

        bestY = int.MaxValue;

        for (int i = 0; i < freeRectangles.Count; ++i)
        {
            // Try to place the rectangle in upright (non-flipped) orientation.
            if (freeRectangles[i].Size.X >= width && freeRectangles[i].Size.Y >= height)
            {
                int topSideY = (int)freeRectangles[i].Position.Y + height;
                if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].Position.X < bestX))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = width;
                    size.Y = height;
                    bestNode.Size = size;
                    bestY = topSideY;
                    bestX = (int)freeRectangles[i].Position.X;
                }
            }
            if (allowRotations && freeRectangles[i].Size.X >= height && freeRectangles[i].Size.Y >= width)
            {
                int topSideY = (int)freeRectangles[i].Position.Y + width;
                if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].Position.X < bestX))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = height;
                    size.Y = width;
                    bestNode.Size = size;
                    bestY = topSideY;
                    bestX = (int)freeRectangles[i].Position.X;
                }
            }
        }
        return bestNode;
    }

    Rect2I FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
    {
        Rect2I bestNode = new Rect2I();
        //memset(&bestNode, 0, sizeof(Rect));

        bestShortSideFit = int.MaxValue;

        for (int i = 0; i < freeRectangles.Count; ++i)
        {
            // Try to place the rectangle in upright (non-flipped) orientation.
            if (freeRectangles[i].Size.X >= width && freeRectangles[i].Size.Y >= height)
            {
                int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].Size.X - width);
                int leftoverVert = Mathf.Abs((int)freeRectangles[i].Size.Y - height);
                int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
                int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

                if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = width;
                    size.Y = height;
                    bestNode.Size = size;
                    bestShortSideFit = shortSideFit;
                    bestLongSideFit = longSideFit;
                }
            }

            if (allowRotations && freeRectangles[i].Size.X >= height && freeRectangles[i].Size.Y >= width)
            {
                int flippedLeftoverHoriz = Mathf.Abs((int)freeRectangles[i].Size.X - height);
                int flippedLeftoverVert = Mathf.Abs((int)freeRectangles[i].Size.Y - width);
                int flippedShortSideFit = Mathf.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                int flippedLongSideFit = Mathf.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = height;
                    size.Y = width;
                    bestNode.Size = size;
                    bestShortSideFit = flippedShortSideFit;
                    bestLongSideFit = flippedLongSideFit;
                }
            }
        }
        return bestNode;
    }

    Rect2I FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
    {
        Rect2I bestNode = new Rect2I();
        //memset(&bestNode, 0, sizeof(Rect));

        bestLongSideFit = int.MaxValue;

        for (int i = 0; i < freeRectangles.Count; ++i)
        {
            // Try to place the rectangle in upright (non-flipped) orientation.
            if (freeRectangles[i].Size.X >= width && freeRectangles[i].Size.Y >= height)
            {
                int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].Size.X - width);
                int leftoverVert = Mathf.Abs((int)freeRectangles[i].Size.Y - height);
                int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
                int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

                if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = width;
                    size.Y = height;
                    bestNode.Size = size;
                    bestShortSideFit = shortSideFit;
                    bestLongSideFit = longSideFit;
                }
            }

            if (allowRotations && freeRectangles[i].Size.X >= height && freeRectangles[i].Size.Y >= width)
            {
                int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].Size.X - height);
                int leftoverVert = Mathf.Abs((int)freeRectangles[i].Size.Y - width);
                int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
                int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

                if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = height;
                    size.Y = width;
                    bestNode.Size = size;
                    bestShortSideFit = shortSideFit;
                    bestLongSideFit = longSideFit;
                }
            }
        }
        return bestNode;
    }

    Rect2I FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
    {
        Rect2I bestNode = new Rect2I();
        //memset(&bestNode, 0, sizeof(Rect));

        bestAreaFit = int.MaxValue;

        for (int i = 0; i < freeRectangles.Count; ++i)
        {
            int areaFit = (int)freeRectangles[i].Size.X * (int)freeRectangles[i].Size.Y - width * height;

            // Try to place the rectangle in upright (non-flipped) orientation.
            if (freeRectangles[i].Size.X >= width && freeRectangles[i].Size.Y >= height)
            {
                int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].Size.X - width);
                int leftoverVert = Mathf.Abs((int)freeRectangles[i].Size.Y - height);
                int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

                if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = width;
                    size.Y = height;
                    bestNode.Size = size;
                    bestShortSideFit = shortSideFit;
                    bestAreaFit = areaFit;
                }
            }

            if (allowRotations && freeRectangles[i].Size.X >= height && freeRectangles[i].Size.Y >= width)
            {
                int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].Size.X - height);
                int leftoverVert = Mathf.Abs((int)freeRectangles[i].Size.Y - width);
                int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

                if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = height;
                    size.Y = width;
                    bestNode.Size = size;
                    bestShortSideFit = shortSideFit;
                    bestAreaFit = areaFit;
                }
            }
        }
        return bestNode;
    }

    /// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
    int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
    {
        if (i1end < i2start || i2end < i1start)
            return 0;
        return Mathf.Min(i1end, i2end) - Mathf.Max(i1start, i2start);
    }

    int ContactPointScoreNode(int x, int y, int width, int height)
    {
        int score = 0;

        if (x == 0 || x + width == binWidth)
            score += height;
        if (y == 0 || y + height == binHeight)
            score += width;

        for (int i = 0; i < usedRectangles.Count; ++i)
        {
            if (usedRectangles[i].Position.X == x + width || usedRectangles[i].Position.X + usedRectangles[i].Size.X == x)
                score += CommonIntervalLength((int)usedRectangles[i].Position.Y, (int)usedRectangles[i].Position.Y + (int)usedRectangles[i].Size.Y, y, y + height);
            if (usedRectangles[i].Position.Y == y + height || usedRectangles[i].Position.Y + usedRectangles[i].Size.Y == y)
                score += CommonIntervalLength((int)usedRectangles[i].Position.X, (int)usedRectangles[i].Position.X + (int)usedRectangles[i].Size.X, x, x + width);
        }
        return score;
    }

    Rect2I FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
    {
        Rect2I bestNode = new Rect2I();
        //memset(&bestNode, 0, sizeof(Rect));

        bestContactScore = -1;

        for (int i = 0; i < freeRectangles.Count; ++i)
        {
            // Try to place the rectangle in upright (non-flipped) orientation.
            if (freeRectangles[i].Size.X >= width && freeRectangles[i].Size.Y >= height)
            {
                int score = ContactPointScoreNode((int)freeRectangles[i].Position.X, (int)freeRectangles[i].Position.Y, width, height);
                if (score > bestContactScore)
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = width;
                    size.Y = height;
                    bestNode.Size = size;
                    bestContactScore = score;
                }
            }
            if (allowRotations && freeRectangles[i].Size.X >= height && freeRectangles[i].Size.Y >= width)
            {
                int score = ContactPointScoreNode((int)freeRectangles[i].Position.X, (int)freeRectangles[i].Position.Y, height, width);
                if (score > bestContactScore)
                {
                    var pos = bestNode.Position;
                    pos.X = freeRectangles[i].Position.X;
                    pos.Y = freeRectangles[i].Position.Y;
                    bestNode.Position = pos;

                    var size = bestNode.Size;
                    size.X = height;
                    size.Y = width;
                    bestNode.Size = size;
                    bestContactScore = score;
                }
            }
        }
        return bestNode;
    }

    bool SplitFreeNode(Rect2I freeNode, ref Rect2I usedNode)
    {
        // Test with SAT if the rectangles even intersect.
        if (usedNode.Position.X >= freeNode.Position.X + freeNode.Size.X || usedNode.Position.X + usedNode.Size.X <= freeNode.Position.X ||
            usedNode.Position.Y >= freeNode.Position.Y + freeNode.Size.Y || usedNode.Position.Y + usedNode.Size.Y <= freeNode.Position.Y)
            return false;

        if (usedNode.Position.X < freeNode.Position.X + freeNode.Size.X && usedNode.Position.X + usedNode.Size.X > freeNode.Position.X)
        {
            // New node at the top side of the used node.
            if (usedNode.Position.Y > freeNode.Position.Y && usedNode.Position.Y < freeNode.Position.Y + freeNode.Size.Y)
            {
                Rect2I newNode = freeNode;
                var size = newNode.Size;
                size.Y = usedNode.Position.Y - newNode.Position.Y;
                newNode.Size = size;
                freeRectangles.Add(newNode);
            }

            // New node at the bottom side of the used node.
            if (usedNode.Position.Y + usedNode.Size.Y < freeNode.Position.Y + freeNode.Size.Y)
            {
                Rect2I newNode = freeNode;
                var pos = newNode.Position;
                pos.Y = usedNode.Position.Y + usedNode.Size.Y;
                newNode.Position = pos;

                var size = newNode.Size;
                size.Y = freeNode.Position.Y + freeNode.Size.Y - (usedNode.Position.Y + usedNode.Size.Y);
                newNode.Size = size;
                freeRectangles.Add(newNode);
            }
        }

        if (usedNode.Position.Y < freeNode.Position.Y + freeNode.Size.Y && usedNode.Position.Y + usedNode.Size.Y > freeNode.Position.Y)
        {
            // New node at the left side of the used node.
            if (usedNode.Position.X > freeNode.Position.X && usedNode.Position.X < freeNode.Position.X + freeNode.Size.X)
            {
                Rect2I newNode = freeNode;
                var size = newNode.Size;
                size.X = usedNode.Position.X - newNode.Position.X;
                newNode.Size = size;
                freeRectangles.Add(newNode);
            }

            // New node at the right side of the used node.
            if (usedNode.Position.X + usedNode.Size.X < freeNode.Position.X + freeNode.Size.X)
            {
                Rect2I newNode = freeNode;
                var pos = newNode.Position;
                pos.X = usedNode.Position.X + usedNode.Size.X;
                newNode.Position = pos;

                var size = newNode.Size;
                size.X = freeNode.Position.X + freeNode.Size.X - (usedNode.Position.X + usedNode.Size.X);
                newNode.Size = size;
                freeRectangles.Add(newNode);
            }
        }

        return true;
    }

    void PruneFreeList()
    {
        for (int i = 0; i < freeRectangles.Count; ++i)
            for (int j = i + 1; j < freeRectangles.Count; ++j)
            {
                if (IsContainedIn(freeRectangles[i], freeRectangles[j]))
                {
                    freeRectangles.RemoveAt(i);
                    --i;
                    break;
                }
                if (IsContainedIn(freeRectangles[j], freeRectangles[i]))
                {
                    freeRectangles.RemoveAt(j);
                    --j;
                }
            }
    }

    bool IsContainedIn(Rect2I a, Rect2I b)
    {
        return a.Position.X >= b.Position.X 
               && a.Position.Y >= b.Position.Y 
               && a.Position.X + a.Size.X <= b.Position.X + b.Size.X
               && a.Position.Y + a.Size.Y <= b.Position.Y + b.Size.Y;
    }

}
