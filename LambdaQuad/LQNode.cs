using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using FeralTic.Resources.Geometry;
using Newtonsoft.Json.Linq;
using SlimDX.Direct3D11;
using VVVV.DX11;
using VVVV.DX11.Lib.Rendering;
// using VVVV.Nodes.PDDN;

using LambdaCube.IR;
using LambdaCube.DX11;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.LambdaQuad
{
    [PluginInfo(Name = "λ⁴", Category = "DX11.Texture", Version = "2d", Author = "microdee", Tags = "LambdaQuad")]
    public class Dx11Texture2DLambdaQuadNode : IPartImportsSatisfiedNotification, IPluginEvaluate, IDX11ResourceProvider
    {
        [Import] public IPluginHost2 FPluginHost;
        [Import] public IIOFactory FioFactory;
        //[Input("Geom")] public Pin<DX11Resource<RawBufferGeometry>> FGeom;

        [Input("Size", StepSize = 1.0)] public IDiffSpread<Vector2D> FSize;
        [Input("Source", StringType = StringType.Filename)] public IDiffSpread<string> FSrc;

        [Output("Texture Out", IsSingle = true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        protected Texture2D Target;
        // protected PinDictionary Pd;
        protected Pipeline LCPipeline;
        protected PipelineInput LCInput;
        protected DX11Pipeline DX11LCPPL;
        protected bool Invalidate = false;

        public void OnImportsSatisfied()
        {
            // Pd = new PinDictionary(FioFactory);
            FSrc.Changed += spread =>
            {
                try
                {
                    string text = System.IO.File.ReadAllText(FSrc[0]);
                    JToken json = JToken.Parse(text);
                    LCPipeline = (Pipeline)Loader.fromJSON(LambdaCube.IR.Type.Pipeline, json);
                    Invalidate = true;
                }
                catch (Exception)
                {
                    
                }
            };
            FSize.Changed += spread =>
            {
                Invalidate = true;
            };
        }

        public void Evaluate(int spreadMax)
        {
            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Texture2D>();
            }
        }

        public void Update(VVVV.PluginInterfaces.V1.IPluginIO pin, DX11RenderContext context)
        {
            if (Invalidate || (DX11LCPPL == null) )
            {
                LCInput = new PipelineInput(context.Device)
                {
                    screenHeight = (int) FSize[0].x,
                    screenWidth = (int) FSize[0].y
                };
                DX11LCPPL = new DX11Pipeline(context.Device, LCPipeline, (int)FSize[0].x, (int)FSize[0].y);
                DX11LCPPL.setPipelineInput(LCInput);

                Target = DX11LCPPL.outRenderTexture;
                if (FTextureOutput[0].Contains(context))
                {
                    FTextureOutput[0].Dispose(context);
                }
                DX11LCPPL.render(context.CurrentDeviceContext);

                ShaderResourceViewDescription desc = new ShaderResourceViewDescription
                {
                    Format = Target.Description.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    MostDetailedMip = 0,
                    MipLevels = 1,
                    ArraySize = 1
                };
                var srv = new ShaderResourceView(context.Device, Target, desc);
                FTextureOutput[0][context] = DX11Texture2D.FromTextureAndSRV(context, Target, srv);
                Invalidate = false;
            }
            else
            {
                DX11LCPPL.render(context.CurrentDeviceContext);
            }
        }

        public void Destroy(VVVV.PluginInterfaces.V1.IPluginIO pin, DX11RenderContext context, bool force)
        {
            FTextureOutput[0].Dispose(context);
            //TODO dispose Pipeline, LCInput
        }
    }
}
