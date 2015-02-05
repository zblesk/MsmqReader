using System;
using System.Data;
using System.Messaging;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace MsmqReader
{
    [DtsPipelineComponent(DisplayName = "MSMQ Reader",
        ComponentType = ComponentType.SourceAdapter,
        IconResource = "MsmqReader.Queue.ico")]
    public class MsmqReader : PipelineComponent
    {
        private const string QueuePathName = "MSMQ path";

        private readonly XmlMessageFormatter _formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
        public int[] MapOutputColsToBufferCols;
        private const string BodyColumnName = "Body";
        private const string LabelColumnName = "Label";
        private const string IdColumnName = "Id";

        public override void PrimeOutput(
            int outputs,
            int[] outputIDs,
            PipelineBuffer[] buffers)
        {
            base.PrimeOutput(outputs, outputIDs, buffers);
            var buffer = buffers[0];

            var qPath = ComponentMetaData.CustomPropertyCollection[QueuePathName].Value.ToString();
            if (!string.IsNullOrEmpty(qPath))
            {
                using (var queue = new MessageQueue(qPath))
                {
                    Message msg;
                    var messages = queue.GetMessageEnumerator2();
                    while (messages.MoveNext())
                    {
                        msg = messages.Current;
                        var message = queue.ReceiveById(msg.Id);
                        if (message == null)
                        {
                            continue;
                        }
                        message.Formatter = _formatter;

                        buffer.AddRow();
                        AddToBuffer(buffer, 0, message.Body);
                        AddToBuffer(buffer, 1, message.Label);
                        AddToBuffer(buffer, 2, message.Id);

                    }
                }
            }
            buffer.SetEndOfRowset();
        }

        private void AddToBuffer(PipelineBuffer buffer, int index, object value)
        {
            if (value == null)
            {
                buffer.SetNull(MapOutputColsToBufferCols[index]);
            }
            else
            {
                buffer[MapOutputColsToBufferCols[index]] = value;
            }
        }

        public override void PreExecute()
        {
            base.PreExecute();
            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];
            MapOutputColsToBufferCols = new int[output.OutputColumnCollection.Count];

            for (int i = 0; i < ComponentMetaData.OutputCollection[0].OutputColumnCollection.Count; i++)
            {
                MapOutputColsToBufferCols[i] = BufferManager.FindColumnByLineageID(
                    output.Buffer,
                    output.OutputColumnCollection[i].LineageID);
            }
        }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            IDTSCustomProperty100 queuePath = ComponentMetaData.CustomPropertyCollection.New();
            queuePath.Description = "The MSMQ path";
            queuePath.Name = QueuePathName;
            queuePath.Value = String.Empty;
            queuePath.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;

            IDTSOutput100 output = ComponentMetaData.OutputCollection.New();
            output.Name = "Output";

            var bodyColumn = ComponentMetaData.OutputCollection[0].OutputColumnCollection.New();
            bodyColumn.Name = BodyColumnName;
            bodyColumn.SetDataTypeProperties(DataType.DT_NTEXT, 0, 0, 0, 0);

            var labelColumn = ComponentMetaData.OutputCollection[0].OutputColumnCollection.New();
            labelColumn.Name = LabelColumnName;
            labelColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);

            var idColumn = ComponentMetaData.OutputCollection[0].OutputColumnCollection.New();
            idColumn.Name = IdColumnName;
            idColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);
        }
    }
}