using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;

namespace SoulBeats
{
    // We are initializing a COM interface for use within the namespace
    // This interface allows access to memory at the byte level which we need to populate audio data that is generated
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public class SoundManager
    {
        private AudioGraph graph;
        private AudioDeviceOutputNode deviceOutputNode;
        private AudioFrameInputNode frameInputNode;
        private double P2 = (Math.PI * 2);

        private bool isPlaying= false;
        private double l_theta = 0;
        private double r_theta = 0;
        private double c_amplitude = 0f;
        private double stepAmplitude = .00003f;
        private double stepFreq = .1f;

        public float l_freq = 500;
        public float r_freq = 500;

        private float cl_freq = 500;
        private float cr_freq = 500;


        public float amplitude = 0.50f;

        public async void init()
        {
            await CreateAudioGraph();
        }
        public void dispose()
        {
            graph.Dispose();
        }

        public void start()
        {
            if (!isPlaying) {
                frameInputNode.Start();
                isPlaying = true;
            }

        }
        public void stop()
        {
            if (isPlaying)
            {
                frameInputNode.Stop();
                isPlaying = false;
            }
        }

        public bool playing() {
            return isPlaying;
        }

        unsafe private AudioFrame GenerateAudioData(uint samples)
        {
            // Buffer size is (number of samples) * (size of each sample) * (number of channels)

            uint bufferSize = samples * sizeof(float);
            AudioFrame frame = new Windows.Media.AudioFrame(bufferSize);

            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // Cast to float since the data we are generating is float
                dataInFloat = (float*)dataInBytes;

                int sampleRate = (int)graph.EncodingProperties.SampleRate;

                // Generate a sine wave and populate the values in the memory buffer
                // nota che i campioni pari sono per il canale SX, e quelli dispari per il canale DX

                for (int i = 0; i < samples; i++)
                {
                    c_amplitude = c_amplitude + stepAmplitude * (Math.Sign(amplitude - c_amplitude));
                    cl_freq = cl_freq + (float)stepFreq * (Math.Sign(l_freq - cl_freq));
                    cr_freq = cr_freq + (float)stepFreq * (Math.Sign(r_freq - cr_freq));

                    dataInFloat[i] = (float)(c_amplitude * Math.Sin(r_theta));

                    i++;
                    dataInFloat[i] = (float)(c_amplitude * Math.Sin(l_theta));

                    l_theta += (cl_freq * P2) / sampleRate;
                    r_theta += (cr_freq * P2) / sampleRate;
                }
            }

            return frame;
        }

        private async Task CreateAudioGraph()
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Cannot create graph
        //        rootPage.NotifyUser(String.Format("AudioGraph Creation Error because {0}", result.Status.ToString()), NotifyType.ErrorMessage);
                return;
            }

            graph = result.Graph;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await graph.CreateDeviceOutputNodeAsync();

            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
//                rootPage.NotifyUser(String.Format("Audio Device Output unavailable because {0}", deviceOutputNodeResult.Status.ToString()), NotifyType.ErrorMessage);
            }

            deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
//            rootPage.NotifyUser("Device Output Node successfully created", NotifyType.StatusMessage);

            // Create the FrameInputNode at the same format as the graph, except explicitly set mono.
            AudioEncodingProperties nodeEncodingProperties = graph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 2;
            frameInputNode = graph.CreateFrameInputNode(nodeEncodingProperties);
            frameInputNode.AddOutgoingConnection(deviceOutputNode);

            // Initialize the Frame Input Node in the stopped state
            frameInputNode.Stop();

            // Hook up an event handler so we can start generating samples when needed
            // This event is triggered when the node is required to provide data
            frameInputNode.QuantumStarted += node_QuantumStarted;

            graph.Start();
        }

        private void node_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            // GenerateAudioData can provide PCM audio data by directly synthesizing it or reading from a file.
            // Need to know how many samples are required. In this case, the node is running at the same rate as the rest of the graph
            // For minimum latency, only provide the required amount of samples. Extra samples will introduce additional latency.
            uint numSamplesNeeded = (uint)args.RequiredSamples * 2;

            if (numSamplesNeeded != 0)
            {
                AudioFrame audioData = GenerateAudioData(numSamplesNeeded);
                frameInputNode.AddFrame(audioData);
            }
        }
    }

}
