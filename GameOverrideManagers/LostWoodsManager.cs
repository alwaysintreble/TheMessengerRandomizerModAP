using System;
using System.Reflection;
using Object = UnityEngine.Object;

namespace MessengerRando.GameOverrideManagers
{
    public static class LostWoodsManager
    {
        public static bool ShouldBeSolved;
        public static bool NeedsSolved;
        private static bool tempSolve;
        public static bool NeedsUnsolved;

        public static void OnSetAsSolved(On.LostWoods.orig_SetAsSolved orig, LostWoods self)
        {
            // if (ShouldBeSolved || tempSolve)
            orig(self);
        }
        public static void SolveLostWoods()
        {
            tempSolve = true;
            NeedsUnsolved = !ShouldBeSolved;
            try
            {
                LostWoods.Instance.SetAsSolved();
                tempSolve = NeedsSolved = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                NeedsSolved = ShouldBeSolved;
                tempSolve = false;
            }
        }

        public static void UnsolveLostWoods()
        {
            var lostWoodsSolved =
                typeof(LostWoods).GetField("solved", BindingFlags.NonPublic | BindingFlags.Instance);
            if (lostWoodsSolved != null)
            {
                lostWoodsSolved.SetValue(Object.FindObjectsOfType<LostWoods>(), true);
                NeedsUnsolved = false;
            }
        }
    }
}