/*
The MIT License (MIT)

Copyright (c) 2007 - 2019 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Models;
using Microting.eFormApi.BasePn.Infrastructure.Database.Base;

namespace Microting.InstallationCheckingBase.Infrastructure.Data.Entities
{
    public class Meter : BaseEntity
    {
        public int Num { get; set; }
        public string QR { get; set; }
        public string RoomType { get; set; }
        public int Floor { get; set; }
        public string RoomName { get; set; }


        [ForeignKey("Installation")]
        public int InstallationId { get; set; }
        public virtual Installation Installation { get; set; }

        public async Task Create(InstallationCheckingPnDbContext dbContext)
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Version = 1;
            WorkflowState = Constants.WorkflowStates.Created;

            dbContext.Meters.Add(this);
            await dbContext.SaveChangesAsync();

            dbContext.MeterVersions.Add(MapVersion(this));
            await dbContext.SaveChangesAsync();
        }

        public async Task Update(InstallationCheckingPnDbContext dbContext)
        {
            Meter meter = await dbContext.Meters.FirstOrDefaultAsync(x => x.Id == Id);
            if (meter == null)
            {
                throw new NullReferenceException($"Could not find item with id: {Id}");
            }

            meter.Num = Num;
            meter.QR = QR;
            meter.RoomType = RoomType;
            meter.Floor = Floor;
            meter.RoomName = RoomName;
            meter.InstallationId = InstallationId;

            if (dbContext.ChangeTracker.HasChanges())
            {
                meter.Version += 1;
                meter.UpdatedAt = DateTime.UtcNow;
                meter.UpdatedByUserId = UpdatedByUserId;

                dbContext.MeterVersions.Add(MapVersion(meter));
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task Delete(InstallationCheckingPnDbContext dbContext)
        {
            Meter meter = await dbContext.Meters.FirstOrDefaultAsync(x => x.Id == Id);
            if (meter == null)
            {
                throw new NullReferenceException($"Could not find item with id: {Id}");
            }

            meter.WorkflowState = Constants.WorkflowStates.Removed;
            
            if (dbContext.ChangeTracker.HasChanges())
            {
                meter.Version += 1;
                meter.UpdatedAt = DateTime.UtcNow;
                meter.UpdatedByUserId = UpdatedByUserId;
                
                dbContext.MeterVersions.Add(MapVersion(meter));
                await dbContext.SaveChangesAsync();
            }
            
        }

        public MeterVersion MapVersion(Meter meter)
        {
            MeterVersion meterVersion = new MeterVersion
            {
                Version = meter.Version,
                CreatedAt = meter.CreatedAt,
                UpdatedAt = meter.UpdatedAt,
                CreatedByUserId = meter.CreatedByUserId,
                UpdatedByUserId = meter.UpdatedByUserId,
                WorkflowState = meter.WorkflowState,
                Num = meter.Num,
                QR = meter.QR,
                RoomType = meter.RoomType,
                Floor = meter.Floor,
                RoomName = meter.RoomName,
                InstallationId = meter.InstallationId,
                Installation = meter.Installation,
                MeterId = meter.Id
            };

            return meterVersion;
        }
    }
}