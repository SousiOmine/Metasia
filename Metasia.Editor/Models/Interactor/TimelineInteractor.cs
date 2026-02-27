using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Models.Interactor
{
    /// <summary>
    /// タイムラインのプロパティ変更に関するビジネスロジックを集約するInteractor
    /// </summary>
    public static class TimelineInteractor
    {
        private static IEnumerable<IMetasiaObject> EnumeratePropertyOwners(ClipObject clip)
        {
            yield return clip;

            if (clip is IAudible audible)
            {
                foreach (var audioEffect in audible.AudioEffects)
                {
                    yield return audioEffect;
                }
            }
            if (clip is IRenderable renderable)
            {
                foreach (var visualEffect in renderable.VisualEffects)
                {
                    yield return visualEffect;
                }
            }
        }

        private static bool TryFindEditableProperty<TProperty>(
            ClipObject clip,
            string propertyIdentifier,
            out IMetasiaObject owner,
            out TProperty propertyValue)
        {
            foreach (var candidate in EnumeratePropertyOwners(clip))
            {
                var property = ObjectPropertyFinder
                    .FindEditableProperties(candidate)
                    .FirstOrDefault(x => x.Identifier == propertyIdentifier);

                if (property?.PropertyValue is TProperty typedValue)
                {
                    owner = candidate;
                    propertyValue = typedValue;
                    return true;
                }
            }

            owner = null!;
            propertyValue = default!;
            return false;
        }

        public static IEditCommand? CreateCoordPointsValueChangeCommand(string propertyIdentifier, CoordPoint targetCoordPoint, double beforeValue, double afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<CoordPointsValueChangeCommand.CoordPointValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<MetaNumberParam<double>>(clip, propertyIdentifier, out _, out var numberParam))
                {
                    continue;
                }

                var points = new List<CoordPoint> { numberParam.StartPoint, numberParam.EndPoint };
                points.AddRange(numberParam.Params);
                CoordPoint? coordPoint = points.FirstOrDefault(x => x.Id == targetCoordPoint.Id);

                if (coordPoint is not null)
                {
                    var valueDifference = afterValue - beforeValue;
                    changeInfos.Add(new CoordPointsValueChangeCommand.CoordPointValueChangeInfo(numberParam, coordPoint, valueDifference));
                }
                // 選択中のクリップにある同一識別子のプロパティで移動が無効であればそっちも動かす
                else if (coordPoint is null && !numberParam.IsMovable)
                {
                    coordPoint = numberParam.StartPoint;
                    var valueDifference = afterValue - beforeValue;
                    changeInfos.Add(new CoordPointsValueChangeCommand.CoordPointValueChangeInfo(numberParam, coordPoint, valueDifference));
                }
            }
            return changeInfos.Count > 0 ? new CoordPointsValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateStringValueChangeCommand(string propertyIdentifier, string beforeValue, string afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<StringValueChangeCommand.StringValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<string>(clip, propertyIdentifier, out var owner, out _))
                {
                    continue;
                }

                changeInfos.Add(new StringValueChangeCommand.StringValueChangeInfo(owner, propertyIdentifier, beforeValue, afterValue));
            }
            return changeInfos.Count > 0 ? new StringValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateFontParamValueChangeCommand(string propertyIdentifier, MetaFontParam beforeValue, MetaFontParam afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<FontParamValueChangeCommand.FontParamValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<MetaFontParam>(clip, propertyIdentifier, out var owner, out _))
                {
                    continue;
                }

                changeInfos.Add(new FontParamValueChangeCommand.FontParamValueChangeInfo(
                    owner,
                    propertyIdentifier,
                    beforeValue.Clone(),
                    afterValue.Clone()));
            }
            return changeInfos.Count > 0 ? new FontParamValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateDoubleValueChangeCommand(string propertyIdentifier, double beforeValue, double afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<DoubleValueChangeCommand.DoubleValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<MetaDoubleParam>(clip, propertyIdentifier, out var owner, out _))
                {
                    continue;
                }

                var valueDifference = afterValue - beforeValue;
                changeInfos.Add(new DoubleValueChangeCommand.DoubleValueChangeInfo(owner, propertyIdentifier, valueDifference));
            }
            return changeInfos.Count > 0 ? new DoubleValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateColorValueChangeCommand(string propertyIdentifier, ColorRgb8 beforeValue, ColorRgb8 afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<ColorValueChangeCommand.ColorValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<ColorRgb8>(clip, propertyIdentifier, out var owner, out _))
                {
                    continue;
                }

                changeInfos.Add(new ColorValueChangeCommand.ColorValueChangeInfo(owner, propertyIdentifier, beforeValue.Clone(), afterValue.Clone()));
            }
            return changeInfos.Count > 0 ? new ColorValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateLayerTargetValueChangeCommand(string propertyIdentifier, LayerTarget beforeValue, LayerTarget afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<LayerTargetValueChangeCommand.LayerTargetValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<LayerTarget>(clip, propertyIdentifier, out var owner, out _))
                {
                    continue;
                }

                changeInfos.Add(new LayerTargetValueChangeCommand.LayerTargetValueChangeInfo(
                    owner,
                    propertyIdentifier,
                    beforeValue.Clone(),
                    afterValue.Clone()));
            }
            return changeInfos.Count > 0 ? new LayerTargetValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateBlendModeValueChangeCommand(string propertyIdentifier, BlendModeKind oldValue, BlendModeKind newValue, IEnumerable<ClipObject> selectedClips)
        {
            List<BlendModeValueChangeCommand.BlendModeValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<BlendModeParam>(clip, propertyIdentifier, out var owner, out _))
                {
                    continue;
                }

                changeInfos.Add(new BlendModeValueChangeCommand.BlendModeValueChangeInfo(
                    owner,
                    propertyIdentifier,
                    oldValue,
                    newValue));
            }
            return changeInfos.Count > 0 ? new BlendModeValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateBoolValueChangeCommand(string propertyIdentifier, bool beforeValue, bool afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<BoolValueChangeCommand.BoolValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                if (!TryFindEditableProperty<bool>(clip, propertyIdentifier, out var owner, out _))
                {
                    continue;
                }

                changeInfos.Add(new BoolValueChangeCommand.BoolValueChangeInfo(owner, propertyIdentifier, beforeValue, afterValue));
            }
            return changeInfos.Count > 0 ? new BoolValueChangeCommand(changeInfos) : null;
        }
    }
}
