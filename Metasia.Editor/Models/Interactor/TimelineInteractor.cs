using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Models.Interactor
{
    /// <summary>
    /// タイムラインのプロパティ変更に関するビジネスロジックを集約するInteractor
    /// </summary>
    public static class TimelineInteractor
    {
        public static IEditCommand? CreateCoordPointsValueChangeCommand(string propertyIdentifier, CoordPoint targetCoordPoint, double beforeValue, double afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<CoordPointsValueChangeCommand.CoordPointValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                var properties = ObjectPropertyFinder.FindEditableProperties(clip);
                var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
                if (property is null || property.PropertyValue!.GetType() != typeof(MetaNumberParam<double>)) continue;
                var numberParam = (MetaNumberParam<double>)property.PropertyValue!;
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
                var properties = ObjectPropertyFinder.FindEditableProperties(clip);
                var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
                if (property is null || property.PropertyValue is not string) continue;

                changeInfos.Add(new StringValueChangeCommand.StringValueChangeInfo(clip, propertyIdentifier, beforeValue, afterValue));
            }
            return changeInfos.Count > 0 ? new StringValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateFontParamValueChangeCommand(string propertyIdentifier, MetaFontParam beforeValue, MetaFontParam afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<FontParamValueChangeCommand.FontParamValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                var properties = ObjectPropertyFinder.FindEditableProperties(clip);
                var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
                if (property is null || property.PropertyValue is not MetaFontParam) continue;

                changeInfos.Add(new FontParamValueChangeCommand.FontParamValueChangeInfo(
                    clip,
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
                var properties = ObjectPropertyFinder.FindEditableProperties(clip);
                var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
                if (property is null || property.PropertyValue is not MetaDoubleParam) continue;

                var valueDifference = afterValue - beforeValue;
                changeInfos.Add(new DoubleValueChangeCommand.DoubleValueChangeInfo(clip, propertyIdentifier, valueDifference));
            }
            return changeInfos.Count > 0 ? new DoubleValueChangeCommand(changeInfos) : null;
        }

        public static IEditCommand? CreateColorValueChangeCommand(string propertyIdentifier, ColorRgb8 beforeValue, ColorRgb8 afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<ColorValueChangeCommand.ColorValueChangeInfo> changeInfos = new();
            foreach (var clip in selectedClips)
            {
                var properties = ObjectPropertyFinder.FindEditableProperties(clip);
                var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
                if (property is null || property.PropertyValue is not ColorRgb8) continue;

                changeInfos.Add(new ColorValueChangeCommand.ColorValueChangeInfo(clip, propertyIdentifier, beforeValue.Clone(), afterValue.Clone()));
            }
            return changeInfos.Count > 0 ? new ColorValueChangeCommand(changeInfos) : null;
        }

        public static bool TryGetDoubleProperty(string propertyIdentifier, ClipObject clip, out double value)
        {
            value = default;

            var properties = ObjectPropertyFinder.FindEditableProperties(clip);
            var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
            if (property is null || property.PropertyValue is not MetaDoubleParam metaDoubleParam)
            {
                return false;
            }

            value = metaDoubleParam.Value;
            return true;
        }
    }
}
