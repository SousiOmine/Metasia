using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.Interactor;

namespace Metasia.Editor.Tests.Models.Interactor
{
    [TestFixture]
    public class ClipInteractorRollEditTests
    {
        private LayerObject _layer;
        private ClipObject _clipA;
        private ClipObject _clipB;

        [SetUp]
        public void Setup()
        {
            _layer = new LayerObject("layer1", "Layer 1");
            // clipA: 0~49, clipB: 50~99 (隣接)
            _clipA = new ClipObject("clipA") { StartFrame = 0, EndFrame = 49 };
            _clipB = new ClipObject("clipB") { StartFrame = 50, EndFrame = 99 };
            _layer.Objects.Add(_clipA);
            _layer.Objects.Add(_clipB);
        }

        #region FindAdjacentClip

        [Test]
        public void FindAdjacentClip_EndHandle_ReturnsAdjacentClip()
        {
            var result = ClipInteractor.FindAdjacentClip(_clipA, "EndHandle", _layer);
            Assert.That(result, Is.EqualTo(_clipB));
        }

        [Test]
        public void FindAdjacentClip_StartHandle_ReturnsAdjacentClip()
        {
            var result = ClipInteractor.FindAdjacentClip(_clipB, "StartHandle", _layer);
            Assert.That(result, Is.EqualTo(_clipA));
        }

        [Test]
        public void FindAdjacentClip_EndHandle_NoAdjacent_ReturnsNull()
        {
            // clipBの後ろにはクリップがない
            var result = ClipInteractor.FindAdjacentClip(_clipB, "EndHandle", _layer);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindAdjacentClip_StartHandle_NoAdjacent_ReturnsNull()
        {
            // clipAの前にはクリップがない
            var result = ClipInteractor.FindAdjacentClip(_clipA, "StartHandle", _layer);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindAdjacentClip_NotAdjacent_ReturnsNull()
        {
            // ギャップがある場合
            _clipB.StartFrame = 51;
            var result = ClipInteractor.FindAdjacentClip(_clipA, "EndHandle", _layer);
            Assert.That(result, Is.Null);
        }

        #endregion

        #region ComputeRollEditFrames

        [Test]
        public void ComputeRollEditFrames_EndHandle_MoveRight()
        {
            // EndHandle を右に10フレーム分ドラッグ (framePerDip=1.0)
            var (newStart, newEnd, adjNewStart, adjNewEnd) =
                ClipInteractor.ComputeRollEditFrames(
                    _clipA, _clipB, "EndHandle",
                    0, 49, 50, 99,
                    10.0, 1.0);

            // clipA: 0~59, clipB: 60~99
            Assert.That(newStart, Is.EqualTo(0));
            Assert.That(newEnd, Is.EqualTo(59));
            Assert.That(adjNewStart, Is.EqualTo(60));
            Assert.That(adjNewEnd, Is.EqualTo(99));
        }

        [Test]
        public void ComputeRollEditFrames_EndHandle_MoveLeft()
        {
            // EndHandle を左に10フレーム分ドラッグ
            var (newStart, newEnd, adjNewStart, adjNewEnd) =
                ClipInteractor.ComputeRollEditFrames(
                    _clipA, _clipB, "EndHandle",
                    0, 49, 50, 99,
                    -10.0, 1.0);

            // clipA: 0~39, clipB: 40~99
            Assert.That(newStart, Is.EqualTo(0));
            Assert.That(newEnd, Is.EqualTo(39));
            Assert.That(adjNewStart, Is.EqualTo(40));
            Assert.That(adjNewEnd, Is.EqualTo(99));
        }

        [Test]
        public void ComputeRollEditFrames_StartHandle_MoveLeft()
        {
            // clipBのStartHandle を左に10フレーム分ドラッグ
            var (newStart, newEnd, adjNewStart, adjNewEnd) =
                ClipInteractor.ComputeRollEditFrames(
                    _clipB, _clipA, "StartHandle",
                    50, 99, 0, 49,
                    -10.0, 1.0);

            // clipB: 40~99, clipA: 0~39
            Assert.That(newStart, Is.EqualTo(40));
            Assert.That(newEnd, Is.EqualTo(99));
            Assert.That(adjNewStart, Is.EqualTo(0));
            Assert.That(adjNewEnd, Is.EqualTo(39));
        }

        [Test]
        public void ComputeRollEditFrames_EndHandle_ClampToMinLength_ClipA()
        {
            // clipA が1フレーム未満にならないよう制約
            // clipA: 0~49, 左に50フレームドラッグ → clipA最小長=0~0
            var (newStart, newEnd, adjNewStart, adjNewEnd) =
                ClipInteractor.ComputeRollEditFrames(
                    _clipA, _clipB, "EndHandle",
                    0, 49, 50, 99,
                    -50.0, 1.0);

            // clipA は最小 0~0 (newEnd=0), clipB は 1~99
            Assert.That(newEnd, Is.EqualTo(0));
            Assert.That(adjNewStart, Is.EqualTo(1));
        }

        [Test]
        public void ComputeRollEditFrames_EndHandle_ClampToMinLength_ClipB()
        {
            // clipB が1フレーム未満にならないよう制約
            // 右に50フレームドラッグ → clipB最小長=98~99
            var (newStart, newEnd, adjNewStart, adjNewEnd) =
                ClipInteractor.ComputeRollEditFrames(
                    _clipA, _clipB, "EndHandle",
                    0, 49, 50, 99,
                    50.0, 1.0);

            // clipB は最小 99~99 → newBoundary = 98
            Assert.That(newEnd, Is.EqualTo(98));
            Assert.That(adjNewStart, Is.EqualTo(99));
        }

        #endregion

        #region CanRollEdit

        [Test]
        public void CanRollEdit_ValidRollEdit_ReturnsTrue()
        {
            // clipA: 0~59, clipB: 60~99
            bool result = ClipInteractor.CanRollEdit(
                _clipA, 0, 59,
                _clipB, 60, 99,
                _layer);

            Assert.That(result, Is.True);
        }

        [Test]
        public void CanRollEdit_OverlappingClips_ReturnsFalse()
        {
            // 重複する場合
            bool result = ClipInteractor.CanRollEdit(
                _clipA, 0, 60,
                _clipB, 50, 99,
                _layer);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CanRollEdit_ZeroLengthClip_ReturnsFalse()
        {
            // newEnd <= newStart の場合
            bool result = ClipInteractor.CanRollEdit(
                _clipA, 50, 49,
                _clipB, 50, 99,
                _layer);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CanRollEdit_NegativeStart_ReturnsFalse()
        {
            bool result = ClipInteractor.CanRollEdit(
                _clipA, -1, 49,
                _clipB, 50, 99,
                _layer);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CanRollEdit_ConflictWithThirdClip_ReturnsFalse()
        {
            // 第三のクリップを追加
            var clipC = new ClipObject("clipC") { StartFrame = 100, EndFrame = 110 };
            _layer.Objects.Add(clipC);

            // clipBを105まで拡張 → clipCと衝突
            bool result = ClipInteractor.CanRollEdit(
                _clipA, 0, 30,
                _clipB, 31, 105,
                _layer);

            Assert.That(result, Is.False);
        }

        #endregion

        #region CreateRollEditCommand

        [Test]
        public void CreateRollEditCommand_Execute_UpdatesBothClips()
        {
            var command = ClipInteractor.CreateRollEditCommand(
                _clipA, 0, 0, 49, 59,
                _clipB, 50, 60, 99, 99);

            command.Execute();

            Assert.That(_clipA.StartFrame, Is.EqualTo(0));
            Assert.That(_clipA.EndFrame, Is.EqualTo(59));
            Assert.That(_clipB.StartFrame, Is.EqualTo(60));
            Assert.That(_clipB.EndFrame, Is.EqualTo(99));
        }

        [Test]
        public void CreateRollEditCommand_Undo_RestoresBothClips()
        {
            var command = ClipInteractor.CreateRollEditCommand(
                _clipA, 0, 0, 49, 59,
                _clipB, 50, 60, 99, 99);

            command.Execute();
            command.Undo();

            Assert.That(_clipA.StartFrame, Is.EqualTo(0));
            Assert.That(_clipA.EndFrame, Is.EqualTo(49));
            Assert.That(_clipB.StartFrame, Is.EqualTo(50));
            Assert.That(_clipB.EndFrame, Is.EqualTo(99));
        }

        [Test]
        public void CreateRollEditCommand_ExecuteUndoRedo_WorksCorrectly()
        {
            var command = ClipInteractor.CreateRollEditCommand(
                _clipA, 0, 0, 49, 59,
                _clipB, 50, 60, 99, 99);

            // Execute
            command.Execute();
            Assert.That(_clipA.EndFrame, Is.EqualTo(59));
            Assert.That(_clipB.StartFrame, Is.EqualTo(60));

            // Undo
            command.Undo();
            Assert.That(_clipA.EndFrame, Is.EqualTo(49));
            Assert.That(_clipB.StartFrame, Is.EqualTo(50));

            // Redo (Execute again)
            command.Execute();
            Assert.That(_clipA.EndFrame, Is.EqualTo(59));
            Assert.That(_clipB.StartFrame, Is.EqualTo(60));
        }

        #endregion
    }
}
